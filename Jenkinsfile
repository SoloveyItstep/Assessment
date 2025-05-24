pipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'sessionmvc-app:latest'
        // Спробуємо іншу, можливо, більш стабільну або нову версію образу docker-compose,
        // якщо docker/compose:latest дасть той самий результат з "Can't find a suitable configuration file"
        // тоді це не в версії проблема, а в тому, як Docker монтує томи або права.
        DOCKER_COMPOSE_IMAGE = 'docker/compose:2.27.0' // Спробуйте конкретну версію, або 'latest'
                                                      // Або навіть повернутися до '1.29.2' для відладки
    }

    stages {
        // ... (попередні етапи без змін) ...

        stage('Start Dependencies') {
            steps {
                echo "DEBUG: Current working directory before docker-compose up:"
                sh 'pwd'
                echo "DEBUG: Listing contents of current directory:"
                sh 'ls -la'

                // Використовуємо стару команду, але з новим ім'ям образу
                sh '''
                    docker run --rm \
                        -v /var/run/docker.sock:/var/run/docker.sock \
                        -v $WORKSPACE:$WORKSPACE \
                        -w $WORKSPACE \
                        ${DOCKER_COMPOSE_IMAGE} \
                        up -d
                '''
            }
        }

        stage('Run App Container') {
            steps {
                sh "docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE"
            }
        }
    }

    post {
        always {
            sh 'docker stop sessionmvc_container || true'
            sh 'docker rm sessionmvc_container || true'

            echo "DEBUG: Current working directory before docker-compose down (post-action):"
            sh 'pwd'
            echo "DEBUG: Listing contents of current directory (post-action):"
            sh 'ls -la'

            sh '''
                docker run --rm \
                    -v /var/run/docker.sock:/var/run/docker.sock \
                    -v $WORKSPACE:$WORKSPACE \
                    -w $WORKSPACE \
                    ${DOCKER_COMPOSE_IMAGE} \
                    down
            '''
        }
    }
}
