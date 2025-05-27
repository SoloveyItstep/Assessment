pipeline {
    agent {
        docker {
            // Використовуємо 8.0, оскільки 9.0 ще в preview, а 8.0 LTS
            image 'mcr.microsoft.com/dotnet/sdk:8.0'
            args '-v $HOME/.nuget:/root/.nuget'
        }
    }

    environment {
        BRANCH_NAME = "${env.GIT_BRANCH_NAME ?: 'master'}"
        DEPLOY_ENV = "Production"
        ASPNETCORE_ENVIRONMENT = "Production"
        IMAGE_TAG_SHORT = sh(returnStdout: true, script: 'git rev-parse --short HEAD').trim()
        IMAGE_TAGS = "sessionmvc:latest, sessionmvc:${env.IMAGE_TAG_SHORT}, sessionmvc:${env.DEPLOY_ENV}-${env.IMAGE_TAG_SHORT}"
    }

    stages {
        stage('Initialize and Display Environment') {
            steps {
                script {
                    echo "Current Git branch (from env.BRANCH_NAME via env.GIT_BRANCH_NAME): ${env.BRANCH_NAME}"
                    echo "Deployment Environment: ${env.DEPLOY_ENV}"
                    echo "ASPNETCORE_ENVIRONMENT for application: ${env.ASPNETCORE_ENVIRONMENT}"
                    echo "Image tags set to: ${env.IMAGE_TAGS}"
                }
            }
        }

        stage('Restore') {
            steps {
                // *** ЗМІНА ТУТ ***
                // Вказуємо шлях до файлу рішення
                sh 'dotnet restore Assessment.sln'
            }
        }

        stage('Build') {
            steps {
                echo "Building solution (Solution: Assessment.sln)..."
                // *** ЗМІНА ТУТ ***
                // Вказуємо шлях до файлу рішення
                sh 'dotnet build Assessment.sln --no-restore --configuration Release'
            }
        }

        stage('Test and Collect Coverage') {
            steps {
                echo "Running .NET tests and collecting coverage (Solution: Assessment.sln)..."
                // *** ЗМІНА ТУТ ***
                // Вказуємо шлях до файлу рішення
                sh 'dotnet test Assessment.sln ' +
                   '--configuration Release ' +
                   '--no-build ' +
                   '/p:CollectCoverage=true ' +
                   '/p:CoverletOutputFormat=cobertura ' +
                   '/p:CoverletOutput=${WORKSPACE}/TestResults/coverage.xml'
            }
            post {
                always {
                    // Переконайтеся, що цей шлях коректний для ваших звітів JUnit
                    // Якщо тести запускаються на рівні рішення, TRX файли можуть бути глибше.
                    // '**/TestResults/**/*.trx' - це гарний варіант, який шукає TRX у всіх піддиректоріях TestResults
                    junit '**/TestResults/**/*.trx'
                }
            }
        }

        stage('Publish Coverage Report') {
            steps {
                cobertura coberturaReportFile: '**/TestResults/coverage.xml',
                          lineCoverageTargets: '80, 90, 95',
                          branchCoverageTargets: '70, 80, 90',
                          failUnhealthy: true,
                          failUnstable: true
            }
        }

        stage('Publish Application') {
            steps {
                echo "Publishing application (Solution: Assessment.sln)..."
                // *** ЗМІНА ТУТ ***
                // Якщо ви публікуєте конкретний проект з рішення, вам потрібно вказати його шлях.
                // Наприклад, 'dotnet publish Assessment/Assessment.csproj --no-build --configuration Release -o app/publish'
                // Якщо Assessment.sln містить лише один publishable проект, то можна і так:
                sh 'dotnet publish Assessment.sln --no-build --configuration Release -o app/publish'
            }
        }

        stage('Build Docker Image') {
            steps {
                echo "Building Docker image (Image Name: sessionmvc)..."
                script {
                    def tags = env.IMAGE_TAGS.split(', ').collect { "-t ${it.trim()}" }.join(' ')
                    sh "docker build . ${tags} -f Dockerfile"
                }
            }
        }

        stage('Push Docker Image (Skipped)') {
            steps {
                echo "Skipping Docker image push for now."
            }
        }

        stage('Deploy to Environment') {
            steps {
                echo "Deploying to ${env.DEPLOY_ENV} environment..."
            }
        }

        stage('Git Tagging for Production') {
            steps {
                echo "Skipping Git tagging for Production for now."
            }
        }
    }

    post {
        always {
            cleanWs()
        }
        success {
            script {
                echo "Pipeline finished successfully for branch ${env.BRANCH_NAME} and environment ${env.DEPLOY_ENV}."
            }
        }
        failure {
            script {
                echo "Pipeline failed for branch ${env.BRANCH_NAME} and environment ${env.DEPLOY_ENV}!"
                // Також перевірте налаштування вашого SMTP-сервера в Jenkins для плагіна пошти.
                // Connection refused вказує на те, що Jenkins не може підключитися до SMTP-сервера на localhost:25.
                // mail(to: 'your_email@example.com', subject: "Jenkins Build Failed: ${env.JOB_NAME} - ${env.BUILD_NUMBER}", body: "Build failed: ${env.BUILD_URL}")
            }
        }
    }
}
