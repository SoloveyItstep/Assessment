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
                // Додаємо --results-directory, щоб результати зберігалися в папку TestResults у корені робочої області
                sh 'dotnet test Assessment.sln --no-build --configuration Release --logger "trx;LogFileName=testresults.trx" --results-directory ./TestResults'
            }
        }
    }
    post {
        always {
            echo 'Pipeline finished.'
            // Вказуємо точніший шлях до файлу результатів тестів
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
