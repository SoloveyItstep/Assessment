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
            agent { label 'master' } // Явно вказуємо виконувати на Jenkins контролері (припускаючи, що його мітка 'master')
            steps {
                echo 'DEBUG: Current PATH on agent for Docker Build:'
                sh 'echo $PATH'
                echo 'DEBUG: Attempting to find docker command:'
                sh 'which docker || echo "docker not found by which"'
                sh 'docker --version || echo "docker --version failed"'
                
                echo 'Building Docker image for SessionMVC...'
                script {
                    try {
                        // Переконуємося, що робоча область доступна
                        // pwd() покаже поточну директорію
                        sh 'pwd' 
                        ls -la // Покаже вміст робочої області
                        
                        // Збираємо образ
                        def appImage = docker.build("sessionmvc-app:${env.BUILD_NUMBER}", "-f SessionMVC/Dockerfile .")
                        echo "Successfully built Docker image: ${appImage.id}"
                    } catch (e) {
                        echo "Error during docker.build: ${e.toString()}"
                        currentBuild.result = 'FAILURE'
                        error "Failed to build Docker image"
                    }
                }
            }
        }

        stage('Run App Docker Image (Test)') {
            agent { label 'master' } // Також на контролері
            steps {
                echo 'Running the SessionMVC Docker image...'
                script {
                    // Перевіряємо, чи існує образ, зібраний на попередньому етапі
                    sh "docker images sessionmvc-app:${env.BUILD_NUMBER}"

                    sh "docker run -d -p 8081:5000 --name sessionmvc-run-${env.BUILD_NUMBER} sessionmvc-app:${env.BUILD_NUMBER}"
                    echo "SessionMVC app should be running on http://localhost:8081"
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
            // Явно вказуємо вузол для виконання post-build дій, щоб мати доступ до робочої області
            agent { label 'master' } 
            steps {
                echo 'Pipeline finished. Archiving test results...'
                junit allowEmptyResults: true, testResults: 'TestResults/testresults.trx'
                recordIssues tool: msBuild(), ignoreQualityGate: true, failOnError: false
            }
        }
        success {
            steps {
                echo 'Pipeline succeeded!'
            }
        }
        failure {
            steps {
                echo 'Pipeline failed!'
            }
        }
    }
}
