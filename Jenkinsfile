pipeline {
    agent any // Глобальний агент. Jenkins виконуватиме кроки на доступному вузлі.

    environment {
        // Базове ім'я образу для вашого додатку
        APP_IMAGE_NAME = 'sessionmvc' // Це ім'я використовується у вашому docker-compose.yml
        // Версія .NET SDK, яку ви використовуєте
        DOTNET_SDK_VERSION = '9.0' // Ви вказали 9.0
        // Email адреса для сповіщень про помилки
        ERROR_NOTIFICATION_EMAIL = 'your-email@example.com' // ЗАМІНІТЬ НА ВАШУ АДРЕСУ
    }

    stages {
        stage('Checkout') {
            steps {
                // Завантажуємо код з репозиторію
                git url: 'https://github.com/SoloveyItstep/Assessment.git', branch: 'master'
                script {
                    // Визначаємо теги для Docker-образу
                    def shortCommit = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                    env.IMAGE_TAG_LATEST = "${env.APP_IMAGE_NAME}:latest"
                    env.IMAGE_TAG_COMMIT = "${env.APP_IMAGE_NAME}:${shortCommit}"
                    
                    echo "Latest image tag: ${env.IMAGE_TAG_LATEST}"
                    echo "Commit image tag: ${env.IMAGE_TAG_COMMIT}"
                }
            }
        }

        stage('Build Application (.NET)') {
            agent {
                docker {
                    image "mcr.microsoft.com/dotnet/sdk:${env.DOTNET_SDK_VERSION}"
                }
            }
            steps {
                echo "Current directory listing inside the container:"
                sh 'ls -la'
                echo "Building the ASP.NET Core application (Solution: Assessment.sln)..."
                sh 'dotnet build Assessment.sln --configuration Release'
            }
        }

        stage('Test Application (.NET)') {
            agent {
                docker {
                    image "mcr.microsoft.com/dotnet/sdk:${env.DOTNET_SDK_VERSION}"
                }
            }
            steps {
                echo "Running .NET tests (Solution: Assessment.sln)..."
                sh 'dotnet test Assessment.sln --configuration Release --no-build'
            }
        }

        stage('Build Docker Image') {
            steps {
                echo "Building Docker image ${env.IMAGE_TAG_LATEST} and ${env.IMAGE_TAG_COMMIT}..."
                sh "docker build -t ${env.IMAGE_TAG_LATEST} -t ${env.IMAGE_TAG_COMMIT} ."
            }
        }

        stage('Push Docker Image (Skipped)') {
            steps {
                echo "Skipping Docker Image Push as per configuration (not using Docker Hub for this app)."
            }
        }

        stage('Deploy with Docker Compose') {
            agent {
                docker {
                    image 'docker/compose:1.29.2'
                    // args '-v /var/run/docker.sock:/var/run/docker.sock' // Розкоментуйте, якщо потрібно
                }
            }
            steps {
                echo 'Deploying application using Docker Compose...'
                sh 'docker-compose --version'
                sh "docker-compose up -d --build sessionmvc"
                echo "To check logs after deploy, run: docker-compose logs --tail=50 sessionmvc"
            }
        }
    }

    post {
        always {
            echo 'Pipeline finished.'
            cleanWs()
        }
        success {
            echo 'Pipeline succeeded!'
            // Приклад сповіщення про успіх (якщо потрібно)
            // mail to: "${env.ERROR_NOTIFICATION_EMAIL}",
            //      subject: "SUCCESS: Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER}",
            //      body: "Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER} completed successfully. URL: ${env.BUILD_URL}"
        }
        failure {
            echo 'Pipeline failed!'
            // Сповіщення про помилку електронною поштою
            mail to: "${env.ERROR_NOTIFICATION_EMAIL}",
                 subject: "FAILURE: Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER}",
                 body: """Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER} failed.
Check console output for more details: ${env.BUILD_URL}console"""
        }
    }
}
