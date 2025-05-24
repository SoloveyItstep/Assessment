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
                // Використовуємо синтаксис плагіна для Declarative Pipeline
                // Зверніть увагу: назва кроку, як правило, просто 'dockerCompose' або 'dockerComposeUp'
                // або 'dockerComposeBuild' залежно від того, як його зареєстровано.
                // Я використовую загальну назву 'dockerCompose' тут.
                // Якщо не спрацює, перевірте Snippet Generator.
                dockerCompose(
                    // Обов'язково вказуємо команду. Для 'up' це зазвичай неявне, але краще вказати.
                    // Це може бути 'up', 'build', 'pull', 'down'
                    command: 'up',
                    // Шлях до файлу docker-compose.yml.
                    // Якщо ваш файл у корені, можна залишити порожнім, або вказати 'docker-compose.yml'.
                    // Зазвичай параметр називається 'file' або 'yamlFile'
                    file: 'docker-compose.yml',
                    // Додаткові опції командного рядка
                    // Ви можете додати --detach або інші опції тут.
                    // Плагін може мати вбудовані параметри, такі як 'detached'
                    options: '--detach'
                )
            }
        }

        stage('Run App Container') {
            steps {
                echo "Running application container..."
                sh "docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE"
            }
        }
    }

    post {
        always {
            echo "Stopping and removing containers..."
            sh 'docker stop sessionmvc_container || true'
            sh 'docker rm sessionmvc_container || true'

            // Використовуємо синтаксис плагіна для Declarative Pipeline для 'down'
            dockerCompose(
                command: 'down',
                file: 'docker-compose.yml',
                // Опції для 'down', наприклад, --volumes
                options: '--volumes'
            )
        }
    }
}
