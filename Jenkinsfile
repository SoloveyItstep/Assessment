pipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'sessionmvc-app:latest'
        // Змінюємо образ Docker Compose на конкретну версію docker/cli
        DOCKER_CLI_IMAGE = 'docker/cli:24.0.9' // Або іншу стабільну версію, яку ви можете знайти на Docker Hub.
                                                // Важливо: переконайтеся, що цей образ має в собі плагін 'compose'.
                                                // Більшість сучасних офіційних образів docker/cli його мають.
        DOCKER_COMPOSE_CUSTOM_IMAGE = 'ubuntu:latest'
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

                // Запускаємо контейнер, встановлюємо docker-compose, а потім виконуємо команду
                sh '''
                    docker run --rm \
                        -v /var/run/docker.sock:/var/run/docker.sock \
                        -v $WORKSPACE:$WORKSPACE \
                        -w $WORKSPACE \
                        ${DOCKER_COMPOSE_CUSTOM_IMAGE} \
                        bash -c " \
                            apt-get update && \
                            apt-get install -y curl && \
                            curl -L https://github.com/docker/compose/releases/download/v2.27.0/docker-compose-$(uname -s)-$(uname -m) -o /usr/local/bin/docker-compose && \
                            chmod +x /usr/local/bin/docker-compose && \
                            docker-compose version && \
                            docker-compose up -d \
                        "
                '''
            }
        }

        stage('Run App Container') {
            steps {
                sh "docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE"
            }
        }
    }

    post {
        always {
            // ...
            sh '''
                docker run --rm \
                    -v /var/run/docker.sock:/var/run/docker.sock \
                    -v $WORKSPACE:$WORKSPACE \
                    -w $WORKSPACE \
                    ${DOCKER_COMPOSE_CUSTOM_IMAGE} \
                    bash -c " \
                        apt-get update && \
                        apt-get install -y curl && \
                        curl -L https://github.com/docker/compose/releases/download/v2.27.0/docker-compose-$(uname -s)-$(uname -m) -o /usr/local/bin/docker-compose && \
                        chmod +x /usr/local/bin/docker-compose && \
                        docker-compose down \
                    "
            '''
        }
    }
}
