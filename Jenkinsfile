pipeline {
    agent none // Глобального агента не визначаємо
    stages {
        stage('.NET Build and Test') {
            agent {
                docker { 
                    image 'mcr.microsoft.com/dotnet/sdk:9.0'
                    // args '-u root' // Розкоментуйте, якщо потрібно
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
            agent { label 'master' } // Явно вказуємо виконувати на Jenkins контролері
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
                        
                        def appImage = docker.build("sessionmvc-app:${env.BUILD_NUMBER}", "-f SessionMVC/Dockerfile .")
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
                    echo 'Running the SessionMVC Docker image...'
                    sh "docker run -d -p 8081:5000 --name sessionmvc-run-${env.BUILD_NUMBER} sessionmvc-app:${env.BUILD_NUMBER}"
                    echo "SessionMVC app should be running on http://localhost:8081"
                    sh "sleep 20" // Даємо час на перевірку
                    echo "Stopping and removing the SessionMVC container..."
                    sh "docker stop sessionmvc-run-${env.BUILD_NUMBER}"
                    sh "docker rm sessionmvc-run-${env.BUILD_NUMBER}"
                    echo "SessionMVC container stopped and removed."
                }
            }
        }
    }
    post {
        always {
            agent { label 'master' } // ВИПРАВЛЕНО: Повертаємо агента для post-дій
            steps { // ВИПРАВЛЕНО: Обгортаємо кроки в steps
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
