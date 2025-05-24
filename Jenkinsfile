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
        // 1) Запускаємо тести з трекером TRX
    sh '''
      dotnet test Assessment.sln --no-build --configuration Release \
        --logger "trx;LogFileName=testresults.trx" --results-directory ./TestResults
    '''

    // 2) Встановлюємо trx2junit (якщо ще не встановлено) і конвертуємо .trx → .xml
    sh '''
      export PATH="$PATH:$HOME/.dotnet/tools"
      if ! command -v trx2junit >/dev/null 2>&1; then
        dotnet tool install --global trx2junit
      fi
      trx2junit ./TestResults/testresults.trx --output ./TestResults/testresults.xml
    '''

        // 3) За бажанням подивитися результат у логах
        sh 'cat ./TestResults/testresults.xml || true'

        // Публікація звіту
        script {
          if (fileExists('TestResults/testresults.xml')) {
            junit 'TestResults/testresults.xml'
          } else {
            echo 'Файл testresults.xml не знайдено. Можливо, тести не були запущені або завершилися з помилкою.'
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
      sh 'docker stop sessionmvc_container || true'
      sh 'docker rm sessionmvc_container || true'
    }
  }
}
