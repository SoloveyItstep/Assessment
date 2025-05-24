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
                // Збираємо образ вашого додатка. Виконується на Jenkins-агенті (який має доступ до Docker).
                sh 'docker build -t <span class="math-inline">DOCKER\_IMAGE \.'
\}
\}
stage\('Start Dependencies'\) \{
// Цей етап запускає docker\-compose\. Ми використовуємо спеціальний Docker\-образ,
// який завантажить та виконає docker\-compose\.
agent \{
docker \{
image "</span>{DOCKER_BASE_IMAGE}"
                    args '-v /var/run/docker.sock:/var/run/docker.sock' // Надаємо доступ до Docker Daemon хоста
                }
            }
            steps {
                echo "DEBUG: Current working directory inside ${DOCKER_BASE_IMAGE} container:"
                sh 'pwd'
                echo "DEBUG: Listing contents inside <span class="math-inline">\{DOCKER\_BASE\_IMAGE\} container\:"
sh 'ls \-la'
echo "Installing Docker Compose v</span>{DOCKER_COMPOSE_VERSION}..."
                sh '''
                    # Встановлюємо curl, необхідний для завантаження docker-compose
                    apk add --no-cache curl

                    # Завантажуємо бінарник Docker Compose v2.x.x
                    # <span class="math-inline">\(uname \-s\)\-</span>(uname -m) автоматично визначить операційну систему та архітектуру (наприклад, linux-x86_64)
                    curl -L "https://github.com/docker/compose/releases/download/v${DOCKER_COMPOSE_VERSION}/docker-compose-linux-<span class="math-inline">\(uname \-m\)" \\\\
\-o /usr/local/bin/docker\-compose
\# Робимо бінарний файл виконуваним
chmod \+x /usr/local/bin/docker\-compose
\# Перевіряємо версію Docker Compose \(для відладки\)
docker\-compose version
echo "Starting Docker Compose services\.\.\."
\# Запускаємо сервіси, визначені в docker\-compose\.yml, у фоновому режимі \(\-d\)
docker\-compose up \-d
'''
\}
\}
stage\('Run App Container'\) \{
// Запускаємо контейнер вашого додатка\.
// Ми можемо використовувати той самий агент, що й для docker\-compose, або будь\-який інший\.
agent \{
docker \{
image "</span>{DOCKER_BASE_IMAGE}" // Використовуємо той же образ для стабільності
                    args '-v /var/run/docker.sock:/var/run/docker.sock'
                }
            }
            steps {
                echo "Running application container..."
                // Запускаємо контейнер sessionmvc, мапимо порт 8081 хоста на 5000 контейнера,
                // і даємо йому ім'я sessionmvc_container.
                sh "docker run -d -p 8081:5000 --name sessionmvc_container
