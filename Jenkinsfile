pipeline {
  agent any

  environment {
    DOCKER_IMAGE = 'sessionmvc-app:latest'
  }

  stages {
    stage('Checkout') {
      steps {
        git url: 'https://github.com/SoloveyItstep/Assessment.git', branch: 'master'
      }
    }

    stage('Restore & Build') {
      agent {
        docker {
          image 'mcr.microsoft.com/dotnet/sdk:9.0'
          args  '-v /var/run/docker.sock:/var/run/docker.sock'
        }
      }
      steps {
        sh 'dotnet restore Assessment.sln'
        sh 'dotnet build  Assessment.sln --configuration Release --no-restore'
      }
    }

    stage('Test') {
      agent {
        docker {
          image 'mcr.microsoft.com/dotnet/sdk:9.0'
          args  '-v /var/run/docker.sock:/var/run/docker.sock'
        }
      }
      steps {
        // Запуск тестів із TRX-логером
        sh '''
          dotnet test Assessment.sln \
            --no-build --configuration Release \
            --logger "trx;LogFileName=testresults.trx" \
            --results-directory ./TestResults
        '''

        // Почистимо старі XML (файли та папки)
        sh 'rm -rf TestResults/*.xml TestResults/*.xml*/'

        // Встановлюємо trx2junit (якщо потрібно) і конвертуємо у JUnit XML
        sh '''
          export PATH="$PATH:$HOME/.dotnet/tools"
          if ! command -v trx2junit >/dev/null 2>&1; then
            dotnet tool install --global trx2junit
          fi
          trx2junit ./TestResults/testresults.trx
        '''

        // Перевіримо, що зʼявився XML-звіт
        sh 'ls -la TestResults'

        // Публікуємо всі JUnit-звіти
        junit 'TestResults/*.xml'
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
      sh 'docker rm   sessionmvc_container || true'
    }
  }
}
