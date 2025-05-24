pipeline {
    agent none // Глобального агента не визначаємо
    stages {
        stage('.NET Build and Test') {
            agent {
                docker { 
                    image 'mcr.microsoft.com/dotnet/sdk:9.0'
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
                        sh 'dotnet test Assessment.sln --no-build --configuration Release --logger "trx;LogFileName=testresults.trx" --results-directory ./TestResults'
                    }
                }
            }
        }

        stage('Build App Docker Image') {
            agent { label 'master' } 
            steps {
                echo 'DEBUG: Current PATH on agent for Docker Build:'
                sh 'echo $PATH'
                echo 'DEBUG: Attempting to find docker command:'
                sh 'which docker' 
                sh 'docker --version' 
                
                echo 'Building Docker image for SessionMVC...'
                script {
                    try {
                        echo "Current directory: ${sh(script: 'pwd', returnStdout: true).trim()}"
                        echo "Workspace contents:"
                        sh 'ls -la' 
                        
                        // Dockerfile тепер у корені
                        def appImage = docker.build("sessionmvc-app:${env.BUILD_NUMBER}", "-f Dockerfile .")
                        echo "Successfully built Docker image: ${appImage.id}"
                    } catch (e) {
                        echo "Error during docker.build: ${e.toString()}"
                        error "Failed to build Docker image: ${e.getMessage()}"
                    }
                }
            }
        }

        stage('Run App Docker Image (Test)') {
            agent { label 'master' } 
            steps {
                script {
                    echo 'Running the SessionMVC Docker image with environment variables...'
                    // ВИПРАВЛЕНО: Додано змінні середовища для рядків підключення
                    // Важливо: паролі та чутливі дані краще передавати через Jenkins Credentials
                    def sqlConnectionString = "Server=db;Database=Assessment;User=sa;Password=Your_password123;Encrypt=False;TrustServerCertificate=True"
                    def mongoConnectionString = "mongodb://mongo:27017/Assessment?directConnection=true" // Замініть Assessment на вашу БД Mongo, якщо потрібно

                    sh """
                        docker run -d \
                            -p 8081:5000 \
                            --name sessionmvc-run-${env.BUILD_NUMBER} \
                            -e ASPNETCORE_ENVIRONMENT=Development \
                            -e "ConnectionStrings__AssessmentDbConnectionString=${sqlConnectionString}" \
                            -e "MongoConnectionString=${mongoConnectionString}" \
                            sessionmvc-app:${env.BUILD_NUMBER}
                    """
                    echo "SessionMVC app starting on http://localhost:8081"
                    echo "Waiting for services to become healthy (this requires other containers like db, mongo, rabbitmq to be running separately if not managed by this Jenkins job)"
                    sh "sleep 30" // Збільшено час очікування, щоб дати шанс базам даних запуститися, ЯКЩО вони запускаються окремо

                    // Перевірка логів контейнера, якщо він впав
                    echo "Checking container status..."
                    sh "docker ps -a --filter name=sessionmvc-run-${env.BUILD_NUMBER}"
                    echo "Fetching logs from sessionmvc-run-${env.BUILD_NUMBER}:"
                    sh "docker logs sessionmvc-run-${env.BUILD_NUMBER} || echo 'Could not fetch logs, container might have exited quickly.'"


                    echo "Stopping and removing the SessionMVC container..."
                    sh "docker stop sessionmvc-run-${env.BUILD_NUMBER} || echo 'Container already stopped or not found'"
                    sh "docker rm sessionmvc-run-${env.BUILD_NUMBER} || echo 'Container already removed or not found'"
                    echo "SessionMVC container stopped and removed."
                }
            }
        }
    }
    post {
        always {
            agent { label 'master' } 
            steps { 
                echo 'Pipeline finished. Processing post-build actions...'
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
