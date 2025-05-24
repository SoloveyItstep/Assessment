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
        // 1) Запустити тести в TRX
        sh '''
          dotnet test Assessment.sln \
            --no-build --configuration Release \
            --logger "trx;LogFileName=testresults.trx" \
            --results-directory ./TestResults
        '''

        // 2) Очистити старий XML
        sh 'rm -f TestResults/testresults.xml'

        // 3) Інсталювати (якщо треба) та конвертувати TRX → JUnit XML
        sh '''
          export PATH="$PATH:$HOME/.dotnet/tools"
          if ! command -v trx2junit >/dev/null 2>&1; then
            dotnet tool install --global trx2junit
          fi
          trx2junit TestResults/testresults.trx --output TestResults/testresults.xml
        '''

        // 4) Опціонально вивести XML для дебагу
        sh 'cat TestResults/testresults.xml || true'

        // 5) Публікація у Jenkins
        junit 'TestResults/testresults.xml'
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
      // Прибирання контейнера
      sh 'docker stop sessionmvc_container || true'
      sh 'docker rm   sessionmvc_container || true'
    }
  }
}
