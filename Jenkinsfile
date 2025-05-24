pipeline {
    agent any // Глобальний агент. Jenkins виконуватиме кроки на доступному вузлі.

    environment {
        // Базове ім'я образу для вашого додатку
        APP_IMAGE_NAME = 'sessionmvc' // Це ім'я використовується у вашому docker-compose.yml
        // Версія .NET SDK, яку ви використовуєте
        DOTNET_SDK_VERSION = '9.0' // Ви вказали 9.0
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
                echo "Building the ASP.NET Core application (Solution: Assessment.sln)..." // Змінено тут
                sh 'dotnet build Assessment.sln --configuration Release'                   // І тут
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
            // Цей етап виконується на агенті Jenkins, який має доступ до Docker CLI
            // та Docker-демону (якщо Jenkins в Docker, сокет має бути прокинутий)
            steps {
                echo "Building Docker image ${env.IMAGE_TAG_LATEST} and ${env.IMAGE_TAG_COMMIT}..."
                // Dockerfile знаходиться в корені проєкту (context: .), тому просто '.'
                // Тегуємо образ одразу двома тегами: 'latest' та з хешем коміту
                sh "docker build -t ${env.IMAGE_TAG_LATEST} -t ${env.IMAGE_TAG_COMMIT} ."
            }
        }

        stage('Push Docker Image (Skipped)') {
            // Цей етап пропускається, оскільки ви не використовуєте Docker Hub для завантаження
            steps {
                echo "Skipping Docker Image Push as per configuration (not using Docker Hub for this app)."
            }
        }

        stage('Deploy with Docker Compose') {
            // Цей етап також потребує доступу до Docker CLI та docker-compose
            steps {
                echo 'Deploying application using Docker Compose...'
                // docker-compose.yml знаходиться в корені проєкту.
                // Команда `docker-compose up -d --build <service_name>` перебудує образ для <service_name>
                // (використовуючи локально зібраний образ, якщо тег співпадає з тим, що вказано в image: у docker-compose,
                // або якщо docker-compose бачить, що build context змінився і його Dockerfile)
                // та перезапустить його.
                // У вашому docker-compose.yml для sessionmvc вказано `image: sessionmvc` та `build: .`,
                // тому `docker-compose up -d --build sessionmvc` повинен використовувати образ,
                // який ми зібрали на попередньому етапі і затегували як 'sessionmvc:latest'.
                sh "docker-compose up -d --build sessionmvc"

                echo "To check logs after deploy, run: docker-compose logs --tail=50 sessionmvc"
            }
        }
    }

    post {
        always {
            echo 'Pipeline finished.'
            // Очищення робочої області Jenkins (видаляє файли з checkout)
            cleanWs()
        }
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            echo 'Pipeline failed!'
            // Тут можна додати сповіщення про помилку
        }
    }
}
