pipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'sessionmvc-app:latest'
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
                echo "Starting Docker Compose dependencies using plugin..."
                // Використовуємо крок плагіна dockerCompose для 'up'
                // Зверніть увагу: dockerCompose.up() або dc.up() можуть бути доступні залежно від версії плагіна.
                // Перевірте Snippet Generator в Jenkins для точного синтаксису.
                dockerCompose.up(
                    file: 'docker-compose.yml', // Шлях до вашого docker-compose.yml
                    detached: true,             // Еквівалент -d
                    removeOrphans: true         // Гарна практика для очищення
                )
            }
        }

        stage('Run App Container') {
            steps {
                echo "Running application container..."
                // Цей етап залишаємо, оскільки це запуск вашого конкретного додатка,
                // який ви збирали в Docker Build stage.
                sh "docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE"
            }
        }
    }

    post {
        always {
            echo "Stopping and removing containers..."
            sh 'docker stop sessionmvc_container || true'
            sh 'docker rm sessionmvc_container || true'

            // Використовуємо крок плагіна dockerCompose для 'down'
            dockerCompose.down(
                file: 'docker-compose.yml',
                removeVolumes: true // Рекомендується для повного очищення
            )
        }
    }
}
