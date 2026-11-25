// Pipeline TicketFlow : vérifier la configuration Docker, construire les images
// du backend et du frontend en parallèle à chaque push, et envoyer un mail après chaque build, réussi ou en échec.
pipeline {
  agent any

  options {
    buildDiscarder(logRotator(numToKeepStr: '10'))
    disableConcurrentBuilds()
    timeout(time: 30, unit: 'MINUTES')
  }

  stages {
    stage('Informations') {
      steps {
        script {
          // L'étiquette des images = identifiant court du commit construit.
          env.TAG = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
        }
        echo "Branche : ${env.BRANCH_NAME ?: 'inconnue'} — commit : ${env.TAG}"
        sh 'docker version'
      }
    }

    stage('Vérifier docker-compose.yml') {
      steps {
        // Échoue si le fichier Compose contient une erreur de syntaxe.
        sh 'docker compose config --quiet'
      }
    }

    // Les deux images sont indépendantes : on les construit en même temps.
    stage('Build des images') {
      // Si l'une des deux constructions échoue, l'autre est arrêtée aussitôt.
      failFast true
      parallel {
        stage('Build backend') {
          steps {
            sh 'docker build -t ticketflow-backend:$TAG backend'
          }
        }
        stage('Build frontend') {
          steps {
            sh 'docker build -t ticketflow-frontend:$TAG frontend'
          }
        }
      }
    }
  }

  post {
    // Build réussi : un mail à chaque fois.
    success {
      echo "Images construites : ticketflow-backend:${env.TAG} et ticketflow-frontend:${env.TAG}"
      mail to: 'siakha.kaba01@gmail.com',
           subject: "SUCCES - ${env.JOB_NAME} #${env.BUILD_NUMBER}",
           body: "Le build ${env.JOB_NAME} #${env.BUILD_NUMBER} a réussi (commit ${env.TAG}).\nImages : ticketflow-backend:${env.TAG} et ticketflow-frontend:${env.TAG}\nBuild : ${env.BUILD_URL}"
    }

    // Build en échec : un mail à chaque fois, avec le lien vers la console.
    failure {
      mail to: 'siakha.kaba01@gmail.com',
           subject: "ECHEC - ${env.JOB_NAME} #${env.BUILD_NUMBER}",
           body: "Le build ${env.JOB_NAME} #${env.BUILD_NUMBER} a échoué (commit ${env.TAG ?: 'inconnu'}).\nConsole : ${env.BUILD_URL}console"
    }
  }
}
