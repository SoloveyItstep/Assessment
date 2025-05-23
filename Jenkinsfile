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
        stage('Test') { // Цей етап спрацює, якщо у вас є тестові проекти в рішенні
            steps {
                echo 'Running tests...'
                sh 'dotnet test Assessment.sln --no-build --configuration Release --logger "trx;LogFileName=testresults.trx"'
            }
        }
    }
    post {
        always {
            echo 'Pipeline finished.'
            // Збереження результатів тестування (якщо є)
            junit allowEmptyResults: true, testResults: '**/*.trx'
            recordIssues tool: msBuild(), failingDisabled: true // failingDisabled: true - щоб збірка не падала через попередження
            // Очищення робочої області
            // cleanWs()
        }
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            echo 'Pipeline failed!'
        }
    }
}
