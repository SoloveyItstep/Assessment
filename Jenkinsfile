pipeline {
    // Глобальний агент можна залишити 'any', якщо Jenkins має доступ до Docker CLI хоста.
    // Або на кожному етапі визначити свого агента.
    agent any

    environment {
        // Змінні середовища, які можуть знадобитися
        // Наприклад, ім'я вашого образу та користувача в Docker Hub (якщо використовуєте)
        // Для тегування образу можна використовувати короткий хеш коміту
        // GIT_SHORT_COMMIT = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
        // IMAGE_NAME = 'sessionmvc'
        // DOCKER_REGISTRY_USER = 'yourdockerhubusername' // Замініть на ваше ім'я користувача
    }

    stages {
        stage('Checkout') {
            steps {
                // Завантажуємо код з репозиторію
                git url: 'https://github.com/SoloveyItstep/Assessment.git', branch: 'master'
                script {
                    // Визначаємо змінні після завантаження коду
                    env.GIT_SHORT_COMMIT = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                    // Замініть 'yourusername/sessionmvc' на ваш реальний шлях до образу (наприклад, Docker Hub username/repository)
                    // Або просто ім'я образу, якщо не використовуєте публічний/приватний реєстр
                    env.IMAGE_NAME_WITH_REGISTRY = "yourusername/sessionmvc" // Наприклад: vsolovey/sessionmvc
                    env.IMAGE_TAG_LATEST = "${env.IMAGE_NAME_WITH_REGISTRY}:latest"
                    env.IMAGE_TAG_COMMIT = "${env.IMAGE_NAME_WITH_REGISTRY}:${env.GIT_SHORT_COMMIT}"
                }
            }
        }

        stage('Build Application (npm)') {
            agent {
                // Використовуємо Docker-контейнер з Node.js для збірки
                docker {
                    image 'node:16-alpine' // Або інша версія Node.js, яка вам потрібна
                    // можна додати args '-u root' якщо є проблеми з правами npm
                }
            }
            steps {
                echo 'Building the application with npm...'
                sh 'npm install'
                sh 'npm run build'
            }
        }

        stage('Test Application (npm)') {
            agent {
                // Використовуємо той самий Docker-контейнер для тестів
                docker {
                    image 'node:16-alpine' // Та сама версія Node.js
                }
            }
            steps {
                echo 'Running tests with npm...'
                sh 'npm run test'
            }
        }

        stage('Build Docker Image') {
            // Цей етап виконується на агенті Jenkins, який має доступ до Docker CLI
            // і Docker-демону (якщо Jenkins в Docker, то сокет має бути прокинутий)
            steps {
                echo "Building Docker image ${env.IMAGE_TAG_LATEST} and ${env.IMAGE_TAG_COMMIT}..."
                // Припускаємо, що Dockerfile знаходиться в корені проєкту
                // Команда docker build -t <ім'я_образу>:<тег> .
                sh "docker build -t ${env.IMAGE_TAG_LATEST} -t ${env.IMAGE_TAG_COMMIT} ."
            }
        }

        stage('Push Docker Image (Optional)') {
            // Цей етап для завантаження образу в Docker-реєстр (наприклад, Docker Hub)
            // Його можна пропустити, якщо ви використовуєте образи локально.
            // Потрібно налаштувати credentials в Jenkins для доступу до реєстру.
            when {
                // Можна додати умову, наприклад, виконувати тільки для гілки master
                // branch 'master'
                expression { true } // Поки що виконується завжди, якщо розкоментовано
            }
            steps {
                echo "Pushing Docker images..."
                // Приклад з використанням Jenkins Credentials Binding plugin для Docker Hub
                // Вам потрібно буде створити 'Username with password' credential в Jenkins
                // з ID, наприклад, 'dockerhub-credentials'
                // withCredentials([usernamePassword(credentialsId: 'dockerhub-credentials', usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                //    sh "echo \"${DOCKER_PASS}\" | docker login -u \"${DOCKER_USER}\" --password-stdin"
                //    sh "docker push ${env.IMAGE_TAG_LATEST}"
                //    sh "docker push ${env.IMAGE_TAG_COMMIT}"
                // }
                // Якщо поки що не налаштували credentials, закоментуйте або видаліть вміст steps
                // Для локального використання push не потрібен.
                echo "Skipping Docker Push for now. Configure credentials if needed."
            }
        }

        stage('Deploy with Docker Compose') {
            // Цей етап також потребує доступу до Docker CLI та docker-compose
            steps {
                echo 'Deploying application using Docker Compose...'
                // Припускаємо, що docker-compose.yml знаходиться в корені проєкту
                // або ви вкажете до нього шлях: -f path/to/your/docker-compose.yml
                
                // Якщо ви завантажили образ в реєстр, і ваш docker-compose.yml використовує це ім'я образу:
                // sh "docker-compose pull sessionmvc" // Завантажує останню версію образу sessionmvc

                // Перезапускає сервіс sessionmvc, перебудовуючи образ, якщо потрібно (згідно з Dockerfile)
                // Якщо ваш docker-compose.yml містить 'build: .' для sessionmvc
                sh "docker-compose up -d --build sessionmvc"
                
                // Або якщо образ вже зібраний і затеганий (і docker-compose.yml посилається на нього, наприклад, sessionmvc:latest):
                // sh "docker-compose up -d --force-recreate sessionmvc"

                // Для оновлення всіх сервісів, визначених у docker-compose.yml:
                // sh "docker-compose up -d --build" // Якщо потрібно перебудувати всі образи з локальним build context
                // sh "docker-compose up -d --force-recreate" // Якщо образи вже є і їх треба просто перезапустити
            }
        }
    }

    post {
        always {
            echo 'Pipeline finished.'
            // Очищення робочої області Jenkins
            cleanWs()
        }
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            echo 'Pipeline failed!'
            // Тут можна додати сповіщення про помилку (наприклад, на email або в Slack)
        }
    }
}
