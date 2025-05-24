pipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'sessionmvc-app:latest'
        // WORKSPACE змінна автоматично доступна в Jenkins, її не потрібно перевизначати.
        // Але її використання є правильним.
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
                    // Важливо: для доступу до Docker daemon зсередини Docker контейнера,
                    // потрібно монтувати сокет. Це вже зроблено правильно.
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
                // Збираємо ваш образ. Важливо, щоб Dockerfile був у корені.
                sh 'docker build -t $DOCKER_IMAGE .'
            }
        }

        stage('Start Dependencies') {
            steps {
                // Піднімаємо Mongo, Redis, RabbitMQ, ваш сервіс за допомогою docker-compose
                // Використання $WORKSPACE тут правильне.
                sh '''
                    docker run --rm \
                        -v /var/run/docker.sock:/var/run/docker.sock \
                        -v $WORKSPACE:$WORKSPACE \
                        -w $WORKSPACE \
                        docker/compose:1.29.2 \
                        up -d
                '''
            }
        }

        stage('Run App Container') {
            steps {
                // Запускаємо вашу апку на 8081
                sh "docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE"
                // Додайте затримку або Health Check, щоб переконатися, що контейнер запущено
                // та він доступний, перш ніж переходити до наступних етапів (якщо такі будуть)
                // наприклад:
                // sh 'sleep 10' // Просто затримка
                // sh 'docker ps -f name=sessionmvc_container' // Перевірка, що контейнер запущено
            }
        }
    }

    post {
        always {
            // Завжди зупиняємо і видаляємо контейнер апки
            sh 'docker stop sessionmvc_container || true'
            sh 'docker rm sessionmvc_container || true'
            // І згортаємо залежності
            sh '''
                docker run --rm \
                    -v /var/run/docker.sock:/var/run/docker.sock \
                    -v $WORKSPACE:$WORKSPACE \
                    -w $WORKSPACE \
                    docker/compose:1.29.2 \
                    down
            '''
        }
    }
}
