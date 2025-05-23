pipeline {
    // Глобального агента не визначаємо, будемо вказувати для груп етапів
    agent none 
    stages {
        stage('.NET Build and Test') { // Групуємо етапи, що потребують .NET SDK
            agent {
                docker { 
                    image 'mcr.microsoft.com/dotnet/sdk:9.0'
                    // args '-u root' // Розкоментуйте, якщо виникають проблеми з правами доступу всередині контейнера
                }
            }
            // Вкладені етапи будуть виконуватися всередині .NET SDK агента
            stages {
                stage('Checkout') {
                    steps {
                        echo 'Cloning the repository...'
                        checkout scm
                    }
                }
                stage('Restore Dependencies') {
                    steps {
                        echo 'Restoring .NET dependencies...'
                        sh 'dotnet restore Assessment.sln'
                    }
                }
                stage('Build') {
                    steps {
                        echo 'Building the project...'
                        sh 'dotnet build Assessment.sln --configuration Release --no-restore'
                    }
                }
                stage('Test') {
                    steps {
                        echo 'Running tests...'
                        // Переконуємося, що результати тестів зберігаються в корені робочої області
                        sh 'dotnet test Assessment.sln --no-build --configuration Release --logger "trx;LogFileName=testresults.trx" --results-directory ./TestResults'
                    }
                }
            }
        } // Кінець етапу '.NET Build and Test'

        stage('Build App Docker Image') {
            // Цей етап буде виконуватися на будь-якому доступному агенті Jenkins, 
            // що має необхідні інструменти. У нашому випадку - на контролері,
            // де ми встановили Docker CLI та промонтували Docker-сокет.
            agent any 
            steps {
                echo 'Building Docker image for SessionMVC...'
                script {
                    // Робоча область (workspace) має бути доступною з попередніх етапів
                    // Команда docker.build використовує Docker CLI, доступний на Jenkins контролері
                    def appImage = docker.build("sessionmvc-app:${env.BUILD_NUMBER}", "-f SessionMVC/Dockerfile .") 
                }
            }
        }

        stage('Run App Docker Image (Test)') {
            agent any // Також на контролері
            steps {
                echo 'Running the SessionMVC Docker image...'
                script {
                    // Прокидаємо порт 5000 контейнера (де слухає додаток) на порт 8081 хоста
                    sh "docker run -d -p 8081:5000 --name sessionmvc-run-${env.BUILD_NUMBER} sessionmvc-app:${env.BUILD_NUMBER}"
                    echo "SessionMVC app should be running on http://localhost:8081"
                    echo "Container will run for a short period for testing and then be stopped."
                    sh "sleep 20" 
                    sh "docker stop sessionmvc-run-${env.BUILD_NUMBER}"
                    sh "docker rm sessionmvc-run-${env.BUILD_NUMBER}"
                    echo "SessionMVC container stopped and removed."
                }
            }
        }
    } // Кінець stages
    post {
        always {
            // Блок post зазвичай виконується на агенті останнього виконаного етапу
            // або на контролері, якщо були проблеми з агентами.
            // Явно вказуємо, що ці кроки мають виконуватися на вузлі з робочою областю
            // для доступу до артефактів, таких як звіти тестів.
            node { 
                echo 'Pipeline finished.'
                junit allowEmptyResults: true, testResults: 'TestResults/testresults.trx'
                recordIssues tool: msBuild(), ignoreQualityGate: true, failOnError: false
            }
        }
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            echo 'Pipeline failed!'
        }
    }
}
