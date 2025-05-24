pipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'sessionmvc-app:latest'
        DOCKER_BASE_IMAGE = 'alpine/git' // Цей образ використовується для виконання команд Docker Compose
        DOCKER_COMPOSE_VERSION = '2.27.0' // Версія Docker Compose, яку ми завантажимо
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
                junit 'TestResults/*.xml'
                sh 'mkdir -p TestResults'
                sh 'rm -f TestResults/testresults.xml'
                sh '''
                    export PATH="$PATH:$HOME/.dotnet/tools"
                    if ! command -v trx2junit >/dev/null 2>&1; then
                        dotnet tool install --global trx2junit
                    fi
                    trx2junit TestResults/testresults.trx
                '''
            }
        }

        stage('Docker Build') {
            steps {
                sh 'docker build -t $DOCKER_IMAGE .'
            }
        }

        stage('Start Dependencies') {
            agent {
                docker {
                    image "${DOCKER_BASE_IMAGE}"
                    args '-v /var/run/docker.sock:/var/run/docker.sock'
                }
            }
            steps {
                echo "DEBUG: Current working directory inside ${DOCKER_BASE_IMAGE} container:"
                sh 'pwd'
                echo "DEBUG: Listing contents inside ${DOCKER_BASE_IMAGE} container:"
                sh 'ls -la'
                echo "Installing Docker Compose v${DOCKER_COMPOSE_VERSION}..."
                sh '''
                    # Встановлюємо curl, необхідний для завантаження docker-compose
                    apk add --no-cache curl

                    # Завантажуємо бінарник Docker Compose v2.x.x
                    curl -L "https://github.com/docker/compose/releases/download/v${DOCKER_COMPOSE_VERSION}/docker-compose-linux-$(uname -m)" \\
                    -o /usr/local/bin/docker-compose

                    # Робимо бінарний файл виконуваним
                    chmod +x /usr/local/bin/docker-compose

                    # Перевіряємо версію Docker Compose (для відладки)
                    docker-compose version

                    echo "Starting Docker Compose services..."
                    # Запускаємо сервіси, визначені в docker-compose.yml, у фоновому режимі (-d)
                    docker-compose up -d
                '''
            }
        }

        stage('Run App Container') {
            agent {
                docker {
                    image "${DOCKER_BASE_IMAGE}"
                    args '-v /var/run/docker.sock:/var/run/docker.sock'
                }
            }
            steps {
                echo "Running application container..."
                sh "docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE"
            }
        }
    }

    post {
        always {
            echo "Stopping and removing containers in post-build action..."
            sh 'docker stop sessionmvc_container || true'
            sh 'docker rm sessionmvc_container || true'

            agent {
                docker {
                    image "${DOCKER_BASE_IMAGE}"
                    args '-v /var/run/docker.sock:/var/run/docker.sock'
                }
            }
            steps {
                echo "Stopping Docker Compose services..."
                sh '''
                    # Встановлюємо curl та docker-compose ще раз на випадок, якщо це окремий виклик
                    # або агент перепідключився. Це забезпечує надійність.
                    apk add --no-cache curl
                    curl -L "https://github.com/docker/compose/releases/download/v${DOCKER_COMPOSE_VERSION}/docker-compose-linux-$(uname -m)" \\
                    -o /usr/local/bin/docker-compose
                    chmod +x /usr/local/bin/docker-compose

                    # Зупиняємо та видаляємо сервіси та їхні томи (--volumes)
                    docker-compose down --volumes
                '''
            }
        }
    }
}
