pipeline {
  agent any

  environment {
    DOCKER_IMAGE = 'sessionmvc-app:latest'
  }

  stages {
    stage('Dotnet Tasks') {
      agent {
        docker {
          image 'mcr.microsoft.com/dotnet/sdk:9.0'
          args '-v /var/run/docker.sock:/var/run/docker.sock'
        }
      }
      steps {
        // 1) Клонування репозиторію
        git url: 'https://github.com/SoloveyItstep/Assessment.git', branch: 'master'

        // 2) Відновлення залежностей
        sh 'dotnet restore Assessment.sln'

        // 3) Побудова проєкту
        sh 'dotnet build Assessment.sln --configuration Release --no-restore'
      }
    }

    stage('Test') {
      steps {
        // 1) Запускаємо тести і генеруємо TRX
        sh '''
          dotnet test Assessment.sln \
            --no-build --configuration Release \
            --logger "trx;LogFileName=testresults.trx" \
            --results-directory ./TestResults
        '''

        // 2) Видаляємо старий XML, щоб trx2junit створив свіжий
        sh 'rm -f ./TestResults/testresults.xml'

        // 3) Встановлюємо trx2junit (якщо потрібно) та конвертуємо TRX → JUnit XML
        sh '''
          export PATH="$PATH:$HOME/.dotnet/tools"
          if ! command -v trx2junit >/dev/null 2>&1; then
            dotnet tool install --global trx2junit
          fi
          trx2junit ./TestResults/testresults.trx --output ./TestResults/testresults.xml
        '''

        // 4) Опціонально вивести для відладки
        sh 'cat ./TestResults/testresults.xml || true'

        // 5) Публікуємо JUnit-звіт лише якщо він існує
        script {
          if (fileExists('TestResults/testresults.xml')) {
            junit 'TestResults/testresults.xml'
          } else {
            error 'JUnit XML report not found – failing the build.'
          }
        }
      }
    }

    stage('Docker Build') {
      steps {
        sh 'docker build -t $DOCKER_IMAGE .'
      }
    }

    stage('Start Dependencies') {
      steps {
        sh 'docker-compose up -d'
      }
    }

    stage('Run App Container') {
      steps {
        sh 'docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE'
      }
    }
  }

  post {
    always {
      // Гарантуємо прибирання контейнера, якщо він запущений
      sh 'docker stop sessionmvc_container || true'
      sh 'docker rm sessionmvc_container || true'
    }
  }
}
