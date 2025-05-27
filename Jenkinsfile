pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:9.0' // Залишаємо .NET 9.0 як ви просили
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

        stage('Install Tools') { // НОВИЙ ЕТАП: Встановлення інструментів
            steps {
                echo "Installing trx2junit global tool..."
                // Встановлюємо trx2junit в контейнер. Цей інструмент конвертує TRX в JUnit XML.
                sh 'dotnet tool install -g trx2junit'
            }
        }

        stage('Restore') {
            steps {
                sh 'dotnet restore Assessment.sln'
            }
        }

        stage('Build') {
            steps {
                echo "Building solution (Solution: Assessment.sln)..."
                sh 'dotnet build Assessment.sln --no-restore --configuration Release'
            }
        }

        stage('Test and Collect Coverage') {
            steps {
                echo "Running .NET tests and collecting coverage (Solution: Assessment.sln)..."
                // Виконуємо тести та збираємо покриття.
                // Не вказуємо VSTestLogger тут, дозволяємо генерувати стандартний TRX файл.
                sh 'dotnet test Assessment.sln ' +
                   '--configuration Release ' +
                   '--no-build ' +
                   '/p:CollectCoverage=true ' +
                   '/p:CoverletOutputFormat=cobertura ' +
                   '/p:CoverletOutput=${WORKSPACE}/TestResults/coverage.xml ' + // Coverlet звіт буде тут
                   '--results-directory "${WORKSPACE}/TestResults"' // Всі результати тестів (включно з TRX) будуть тут
            }
            post {
                always {
                    echo "Listing contents of TestResults directory for conversion:"
                    sh "ls -R ${WORKSPACE}/TestResults" // Виведе вміст для перевірки

                    // Конвертуємо TRX файл у JUnit XML
                    script {
                        def trxFile = "${WORKSPACE}/TestResults/TestResults.xml"
                        def junitFile = "${WORKSPACE}/TestResults/junit.xml"

                        // Перевіряємо, чи існує TRX файл перед конвертацією
                        if (fileExists(trxFile)) {
                            echo "Converting TRX report to JUnit XML: ${trxFile} -> ${junitFile}"
                            // Використовуємо trx2junit для конвертації
                            // '>' перенаправляє вивід в junit.xml
                            sh "trx2junit ${trxFile} > ${junitFile}"

                            echo "Listing contents of TestResults after conversion:"
                            sh "ls -R ${WORKSPACE}/TestResults" // Перевірка, чи з'явився junit.xml

                            // Публікуємо JUnit звіт
                            junit "${junitFile}" // Вказуємо шлях до згенерованого JUnit XML файлу
                        } else {
                            // Якщо TRX файл не знайдено, білд позначиться як FAILURE.
                            error "TRX test results file not found at ${trxFile}. Cannot publish JUnit report."
                        }
                    }
                }
            }
        }

        stage('Publish Coverage Report') {
            steps {
                // Публікуємо звіт покриття за допомогою Cobertura Plugin
                // Цей етап тепер повинен спрацювати, оскільки проблема з JUnit виправлена
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
                // Якщо ви хочете опублікувати конкретний проект з рішення:
                // sh 'dotnet publish Assessment/Assessment.csproj --no-build --configuration Release -o app/publish'
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
                // Ваша помилка з відправкою пошти (Connection refused) не стосується .NET,
                // а пов'язана з налаштуванням SMTP-сервера в Jenkins.
                // Перевірте Manage Jenkins -> Configure System -> Email Notification.
                // mail(to: 'your_email@example.com', subject: "Jenkins Build Failed: ${env.JOB_NAME} - ${env.BUILD_NUMBER}", body: "Build failed: ${env.BUILD_URL}")
            }
        }
    }
}
