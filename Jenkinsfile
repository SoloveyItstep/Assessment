pipeline {
  agent any

  environment {
    DOCKER_IMAGE = 'sessionmvc-app:latest'
  }

  stages {
    stage('Checkout') {
      steps {
        // Клонування коду в корінь воркспейсу
        checkout scm
      }
    }

    stage('Build & Test') {
      // Виконуємо в .NET SDK контейнері
      agent {
        docker {
          image 'mcr.microsoft.com/dotnet/sdk:9.0'
          args  '-v /var/run/docker.sock:/var/run/docker.sock'
        }
      }
      steps {
        sh 'dotnet restore Assessment.sln'
        sh 'dotnet build Assessment.sln --configuration Release --no-restore'

        // Запускаємо тести з trx-логом
        sh '''
          dotnet test Assessment.sln \
            --no-build --configuration Release \
            --logger "trx;LogFileName=testresults.trx" \
            --results-directory TestResults
        '''

        // Конвертуємо trx → junit
        sh 'mkdir -p TestResults && rm -f TestResults/*.xml'
        sh '''
          export PATH="$PATH:$HOME/.dotnet/tools"
          if ! command -v trx2junit >/dev/null 2>&1; then
            dotnet tool install --global trx2junit
          fi
          trx2junit TestResults/testresults.trx
        '''

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
        script {
          // Отримуємо ID поточного Jenkins-контейнера
          def jenkinsCid = sh(script: 'hostname', returnStdout: true).trim()

          // Запускаємо docker-compose у окремому контейнері, змонтувавши робочу теку
          sh """
            docker run --rm \\
              --volumes-from ${jenkinsCid} \\
              -v /var/run/docker.sock:/var/run/docker.sock \\
              -w ${env.WORKSPACE} \\
              docker/compose:2.20.2 up -d
          """
        }
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
      // Зупинка та видалення контейнера додатку
      sh 'docker stop sessionmvc_container || true'
      sh 'docker rm   sessionmvc_container   || true'

      // Згортаємо залежності через той самий образ compose
      script {
        def jenkinsCid = sh(script: 'hostname', returnStdout: true).trim()
        sh """
          docker run --rm \\
            --volumes-from ${jenkinsCid} \\
            -v /var/run/docker.sock:/var/run/docker.sock \\
            -w ${env.WORKSPACE} \\
            docker/compose:2.20.2 down || true
        """
      }
    }
  }
}
