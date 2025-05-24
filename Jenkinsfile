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
                echo "Starting Docker Compose dependencies using plugin (via script block)..."
                script {
                    // Виклик плагіна Docker Compose Build Step
                    // Перевірте Snippet Generator для точного синтаксису,
                    // але це має бути щось на кшталт:
                    // step([$class: 'DockerComposeBuilder',
                    //       dockerComposeFile: 'docker-compose.yml',
                    //       executeCommandInsideContainer: [],
                    //       startAllServices: true, // Це для 'up -d'
                    //       stopAllServices: false // Не зупиняємо тут
                    // ])

                    // Краще використовувати параметри, які імітують команду `up -d`
                    // Це може бути або 'StartAllServices', або 'ExecuteCommandInsideContainer'
                    // Давайте використаємо ExecuteCommandInsideContainer, щоб бути гнучкими
                    step([$class: 'DockerComposeBuilder',
                          // Обираємо команду для виконання
                          dockerComposeCommand: [
                              $class: 'ExecuteCommandInsideContainer', // Це виконує довільну команду
                              command: 'up -d', // Саме тут передаємо команду Docker Compose
                              // service: '', // Якщо ви не вказуєте конкретний сервіс, то команду виконують для всіх.
                              // workDir: '' // Робочий каталог, якщо docker-compose.yml не в корені.
                          ],
                          dockerComposeFile: 'docker-compose.yml' // Шлях до файлу docker-compose.yml
                    ])
                }
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

            script {
                // Виклик плагіна Docker Compose Build Step для 'down'
                step([$class: 'DockerComposeBuilder',
                      dockerComposeCommand: [
                          $class: 'ExecuteCommandInsideContainer',
                          command: 'down --volumes' // Команда down з опцією видалення томів
                      ],
                      dockerComposeFile: 'docker-compose.yml'
                ])
            }
        }
    }
}
