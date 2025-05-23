pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:9.0' // Використовуємо офіційний .NET 9 SDK образ
            // args '-u root' // Розкоментуйте, якщо виникають проблеми з правами доступу всередині контейнера
        }
    }
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
                sh 'dotnet restore Assessment.sln' // Вкажіть шлях до вашого .sln файлу
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
                sh 'dotnet test Assessment.sln --no-build --configuration Release --logger "trx;LogFileName=testresults.trx" --results-directory ./TestResults'
            }
        }
        stage('Build App Docker Image') {
            steps {
                echo 'Building Docker image for SessionMVC...'
                script {
                    // Збираємо Docker-образ, вказуючи шлях до Dockerfile та контекст збірки
                    // -f SessionMVC/Dockerfile  <-- вказує на ваш Dockerfile
                    // .                         <-- контекст збірки (корінь репозиторію)
                    def appImage = docker.build("sessionmvc-app:${env.BUILD_NUMBER}", "-f SessionMVC/Dockerfile .") 
                }
            }
        }
        stage('Run App Docker Image (Test)') {
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
    }
    post {
        always {
            echo 'Pipeline finished.'
            junit allowEmptyResults: true, testResults: 'TestResults/testresults.trx'
            recordIssues tool: msBuild(), ignoreQualityGate: true, failOnError: false
        }
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            echo 'Pipeline failed!'
        }
    }
}
