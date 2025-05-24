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
                    args '-v /var/run/docker.sock:/var/run/docker.sock' // Для доступу до Docker Daemon з контейнера SDK
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
                junit 'TestResults/*.xml' // Перемістив junit сюди, щоб він виконувався після тестів
                sh 'mkdir -p TestResults' // Переконайтеся, що папка створюється перед використанням
                sh 'rm -f TestResults/testresults.xml' // Очищаємо перед використанням trx2junit
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
                // Збираємо образ вашого додатка. Виконується на Jenkins-агенті (який має доступ до Docker).
                sh 'docker build -t $DOCKER_IMAGE .' // Перевірте цей рядок!
            }
        }

        stage('Start Dependencies') {
            // Цей етап запускає docker-compose. Ми використовуємо спеціальний Docker-образ,
            // який завантажить та виконає docker-compose.
            agent {
                docker {
                    image "${DOCKER_BASE_IMAGE}"
                    args '-v /var/run/docker.sock:/var/run/docker.sock' // Надаємо доступ до Docker Daemon хоста
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
                    # $(uname -s)-$(uname -m) автоматично визначить операційну систему та архітектуру (наприклад, linux-x86_64)
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
            // Запускаємо контейнер вашого додатка.
            // Ми можемо використовувати той самий агент, що й для docker-compose, або будь-який інший.
            agent {
                docker {
                    image "${DOCKER_BASE_IMAGE}" // Використовуємо той же образ для стабільності
                    args '-v /var/run/docker.sock:/var/run/docker.sock'
                }
            }
            steps {
                echo "Running application container..."
                // Запускаємо контейнер sessionmvc, мапимо порт 8081 хоста на 5000 контейнера,
                // і даємо йому ім'я sessionmvc_container.
                sh "docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE" // Перевірте цей рядок!
            }
        }
    }

    post {
        always {
            echo "Stopping and removing containers in post-build action..."
            // Зупиняємо та видаляємо контейнер додатка. '|| true' запобігає падінню пайплайну,
            // якщо контейнер вже не існує (наприклад, попередній запуск не вдався повністю).
            sh 'docker stop sessionmvc_container || true'
            sh 'docker rm sessionmvc_container || true'

            // Зупиняємо та видаляємо сервіси, запущені за допомогою docker-compose.
            agent {
                docker {
                    image "${DOCKER_BASE_IMAGE}"
                    args '-v /var/run/docker.sock:/var/run/docker.sock'
                }
            }
            steps {
                echo "Stopping Docker Compose services..."
                # Встановлюємо curl та docker-compose ще раз на випадок, якщо це окремий виклик
                # або агент перепідключився. Це забезпечує надійність.
                sh '''
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
