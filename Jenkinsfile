pipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'sessionmvc-app:latest'
        // Оновимо версію docker-compose.
        // Замість docker/compose:1.29.2 використовуємо docker/cli:latest
        // і викликаємо docker compose як плагін.
        DOCKER_COMPOSE_CLI_IMAGE = 'docker/cli:latest' // Або конкретна версія, наприклад, 'docker/cli:20.10.17'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build & Test') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:9.0'
                    args '-v /var/run/docker.sock:/var/run/docker.sock'
                }
            }
            steps {
                sh 'dotnet restore Assessment.sln'
                sh 'dotnet build Assessment.sln --configuration Release --no-restore'
                sh '''
                    dotnet test Assessment.sln \\
                      --no-build --configuration Release \\
                      --logger "trx;LogFileName=testresults.trx" \\
                      --results-directory TestResults
                '''
                sh 'mkdir -p TestResults'
                sh 'rm -f TestResults/testresults.xml'
                sh '''
                    export PATH="$PATH:$HOME/.dotnet/tools"
                    if ! command -v trx2junit >/dev/null 2>&1; then
                        dotnet tool install --global trx2junit
                    fi
                    trx2junit TestResults/testresults.trx
                '''
                junit 'TestResults/*.xml'
            }
        }

        stage('Docker Build') {
            steps {
                sh 'docker build -t $DOCKER_IMAGE .'
            }
        }

        stage('Start Dependencies') {
            steps {
                echo "DEBUG: Current working directory before docker-compose up:"
                sh 'pwd'
                echo "DEBUG: Listing contents of current directory:"
                sh 'ls -la'

                // Змінена команда для запуску docker-compose через docker/cli
                sh '''
                    docker run --rm \
                        -v /var/run/docker.sock:/var/run/docker.sock \
                        -v $WORKSPACE:$WORKSPACE \
                        -w $WORKSPACE \
                        ${DOCKER_COMPOSE_CLI_IMAGE} \
                        compose up -d
                '''
            }
        }

        stage('Run App Container') {
            steps {
                sh "docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE"
                // Оскільки healthchecks налаштовані в docker-compose, чекати окремо не потрібно.
                // Якщо раптом виникнуть проблеми зі стартом, можна додати sleep або wait-for-it.
            }
        }
    }

    post {
        always {
            sh 'docker stop sessionmvc_container || true'
            sh 'docker rm sessionmvc_container || true'

            echo "DEBUG: Current working directory before docker-compose down (post-action):"
            sh 'pwd'
            echo "DEBUG: Listing contents of current directory (post-action):"
            sh 'ls -la'

            // Змінена команда для зупинки docker-compose через docker/cli
            sh '''
                docker run --rm \
                    -v /var/run/docker.sock:/var/run/docker.sock \
                    -v $WORKSPACE:$WORKSPACE \
                    -w $WORKSPACE \
                    ${DOCKER_COMPOSE_CLI_IMAGE} \
                    compose down
            '''
        }
    }
}
