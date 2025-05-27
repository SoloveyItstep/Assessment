pipeline {
    agent {
        // Визначаємо Docker образ для всього пайплайну
        // Це гарантує, що команда 'dotnet' буде доступна на будь-якому етапі
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:9.0'
            args '-v $HOME/.nuget:/root/.nuget' // Дозволяє кешувати NuGet пакети поза контейнером, прискорюючи restore
        }
    }

    environment {
        // Визначення змінних середовища для всього пайплайну
        BRANCH_NAME = "${env.GIT_BRANCH_NAME ?: 'master'}" // Якщо у вас мульти-гілковий пайплайн, використовує ім'я гілки
        DEPLOY_ENV = "Production" // Або інша змінна для середовища розгортання
        ASPNETCORE_ENVIRONMENT = "Production" // Для конфігурації ASP.NET Core
        // Отримання короткого Git-хешу для тегів образів Docker
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
                sh 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                // Збираємо рішення, не відновлюючи пакети повторно
                sh 'dotnet build --no-restore --configuration Release'
            }
        }

        stage('Test and Collect Coverage') {
            steps {
                echo "Running .NET tests and collecting coverage (Solution: Assessment.sln)..."
                // Виконуємо тести та збираємо покриття в один крок
                // Зверніть увагу: шлях до TestResults/coverage.xml відносно WORKSPACE
                sh 'dotnet test Assessment.sln ' +
                   '--configuration Release ' +
                   '--no-build ' + // Не перезбирати проект, оскільки ми його вже зібрали на етапі 'Build'
                   '/p:CollectCoverage=true ' +
                   '/p:CoverletOutputFormat=cobertura ' +
                   '/p:CoverletOutput=${WORKSPACE}/TestResults/coverage.xml'
            }
            post {
                always {
                    // Публікуємо результати JUnit тестів.
                    // **Примітка:** Ваш лог показує "No test report files were found".
                    // Можливо, шлях до .trx файлів потребує уточнення.
                    // Зазвичай вони знаходяться у піддиректорії: TestResults/<Назва_Тестового_Проекту>/<Унікальний_GUID>/*.trx
                    // '**/TestResults/**/*.trx' - шукає .trx файли у будь-якій піддиректорії TestResults
                    junit '**/TestResults/**/*.trx'
                }
            }
        }

        stage('Publish Coverage Report') {
            steps {
                // Публікуємо звіт покриття за допомогою Cobertura Plugin
                // '**/TestResults/coverage.xml' - шукає звіт у будь-якій піддиректорії TestResults
                cobertura coberturaReportFile: '**/TestResults/coverage.xml',
                          lineCoverageTargets: '80, 90, 95',
                          branchCoverageTargets: '70, 80, 90',
                          failUnhealthy: true,
                          failUnstable: true
            }
        }

        stage('Publish Application') { // Перейменовано для ясності
            steps {
                echo "Publishing application (Solution: Assessment.sln)..."
                // Публікуємо ваш основний проект. Переконайтеся, що Assessment.sln включає ваш основний проект.
                sh 'dotnet publish Assessment.sln --no-build --configuration Release -o app/publish'
            }
        }

        stage('Build Docker Image') {
            steps {
                echo "Building Docker image (Image Name: sessionmvc)..."
                // Припускаємо, що ваш Dockerfile знаходиться у корені проекту
                script {
                    // Формуємо список тегів для команди docker build
                    def tags = env.IMAGE_TAGS.split(', ').collect { "-t ${it.trim()}" }.join(' ')
                    sh "docker build . ${tags} -f Dockerfile"
                }
            }
        }

        stage('Push Docker Image (Skipped)') { // Залишаємо назву, як у вас
            // Тут ви б додавали логіку для push до Docker Registry
            steps {
                echo "Skipping Docker image push for now."
            }
        }

        stage('Deploy to Environment') {
            // Ваша логіка розгортання
            steps {
                echo "Deploying to ${env.DEPLOY_ENV} environment..."
            }
        }

        stage('Git Tagging for Production') {
            // Ваша логіка тегування Git
            steps {
                echo "Skipping Git tagging for Production for now."
            }
        }
    }

    post {
        always {
            cleanWs() // Очищуємо робочу область після кожного білду
        }
        success {
            script {
                echo "Pipeline finished successfully for branch ${env.BRANCH_NAME} and environment ${env.DEPLOY_ENV}."
            }
        }
        failure {
            script {
                echo "Pipeline failed for branch ${env.BRANCH_NAME} and environment ${env.DEPLOY_ENV}!"
                // Ваш mail плагін видає Connection refused. Перевірте налаштування SMTP у Jenkins.
                // mail(to: 'your_email@example.com', subject: "Jenkins Build Failed: ${env.JOB_NAME} - ${env.BUILD_NUMBER}", body: "Build failed: ${env.BUILD_URL}")
            }
        }
    }
}
