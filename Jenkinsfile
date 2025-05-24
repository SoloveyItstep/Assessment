pipeline {
    agent any // Глобальний агент. Можна змінити на 'none' і визначати агент для кожного етапу

    environment {
        // Визначаємо імена образів та теги тут, щоб їх було легко змінити
        // Це базове ім'я образу для вашого додатку
        APP_IMAGE_NAME = 'sessionmvc'
        // Якщо ви плануєте використовувати Docker Hub або інший реєстр, вкажіть повне ім'я:
        // DOCKER_REGISTRY_IMAGE_NAME = "yourdockerhubusername/${APP_IMAGE_NAME}"
        // Якщо реєстр не використовується, залиште просто APP_IMAGE_NAME
        // Для прикладу, якщо не використовуємо реєстр:
        FINAL_IMAGE_NAME_BASE = APP_IMAGE_NAME
    }

    stages {
        stage('Checkout') {
            steps {
                git url: 'https://github.com/SoloveyItstep/Assessment.git', branch: 'master'
                script {
                    // Визначаємо теги після завантаження коду
                    def shortCommit = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                    env.IMAGE_TAG_LATEST = "${env.FINAL_IMAGE_NAME_BASE}:latest"
                    env.IMAGE_TAG_COMMIT = "${env.FINAL_IMAGE_NAME_BASE}:${shortCommit}"
                }
            }
        }

        // --- Етапи для ASP.NET Core проєкту ---
        stage('Build Application (.NET)') {
            agent {
                // Використовуємо Docker-контейнер з .NET SDK 9.0 для збірки
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:9.0'
                    // args '-u root' // Якщо є проблеми з правами доступу
                }
            }
            steps {
                echo 'Building the ASP.NET Core application...'
                // Потрібно знати шлях до .sln або .csproj файлу, якщо він не в корені
                // Якщо .sln або .csproj в корені:
                sh 'dotnet build --configuration Release'
                // Якщо в підпапці, наприклад, src/YourProject.sln:
                // sh 'dotnet build src/YourProject.sln --configuration Release'
            }
        }

        stage('Test Application (.NET)') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:9.0' // Та сама версія SDK
                }
            }
            steps {
                echo 'Running .NET tests...'
                // Потрібно знати шлях до тестового проєкту або .sln
                // Якщо тести визначаються у .sln:
                sh 'dotnet test --configuration Release --no-build'
                // Якщо для конкретного тестового проєкту:
                // sh 'dotnet test path/to/your/testproject.csproj --configuration Release --no-build'
            }
        }
        // --- Кінець етапів для ASP.NET Core проєкту ---

        stage('Build Docker Image') {
            steps {
                echo "Building Docker image ${env.IMAGE_TAG_LATEST} and ${env.IMAGE_TAG_COMMIT}..."
                // Dockerfile знаходиться в корені проєкту (context: .)
                // Згідно з docker-compose.yml, образ має називатися 'sessionmvc'
                sh "docker build -t ${env.IMAGE_TAG_LATEST} -t ${env.IMAGE_TAG_COMMIT} ."
            }
        }

        stage('Push Docker Image (Optional)') {
            // Розкоментуйте та налаштуйте, якщо будете використовувати Docker Registry
            /*
            when {
                branch 'master' // Наприклад, пушити тільки для гілки master
            }
            steps {
                echo "Pushing Docker images..."
                // Потрібно налаштувати credentials в Jenkins (наприклад, 'dockerhub-credentials')
                withCredentials([usernamePassword(credentialsId: 'dockerhub-credentials', usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                   sh "echo \"${DOCKER_PASS}\" | docker login -u \"${DOCKER_USER}\" --password-stdin your.registry.com" // Замініть your.registry.com, якщо не Docker Hub
                   sh "docker push ${env.IMAGE_TAG_LATEST}" // Або env.DOCKER_REGISTRY_IMAGE_NAME
                   sh "docker push ${env.IMAGE_TAG_COMMIT}" // Або env.DOCKER_REGISTRY_IMAGE_NAME
                }
            }
            */
            steps {
                echo "Skipping Docker Push. Configure if needed."
            }
        }

        stage('Deploy with Docker Compose') {
            steps {
                echo 'Deploying application using Docker Compose...'
                // docker-compose.yml знаходиться в корені
                sh "docker-compose up -d --build sessionmvc"
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
        }
        failure {
            echo 'Pipeline failed!'
        }
    }
}
