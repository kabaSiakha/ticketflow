// Pipeline TicketFlow : vérifier la configuration Docker, construire les images
// du backend et du frontend en parallèle à chaque push, et prévenir par mail en cas d'échec.
pipeline {
  agent any

  environment {
    // Destinataire des notifications
    NOTIFY_EMAIL = 'siakha.kaba01@gmail.com'
  }

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
    success {
      echo "Images construites : ticketflow-backend:${env.TAG} et ticketflow-frontend:${env.TAG}"
    }

    // Build en échec : mail avec le lien vers la console.
    failure {
      mail(
        to: env.NOTIFY_EMAIL,
        subject: "ÉCHEC — ${env.JOB_NAME} #${env.BUILD_NUMBER}",
        mimeType: 'text/html',
        body: """
          <p>Le build <b>${env.JOB_NAME} #${env.BUILD_NUMBER}</b> a échoué.</p>
          <p>Commit : ${env.TAG ?: 'inconnu'}</p>
          <p><a href="${env.BUILD_URL}console">Ouvrir la console du build</a></p>
        """
      )
    }

    // Premier build vert après un échec : mail de rétablissement.
    fixed {
      mail(
        to: env.NOTIFY_EMAIL,
        subject: "RÉTABLI — ${env.JOB_NAME} #${env.BUILD_NUMBER}",
        mimeType: 'text/html',
        body: """
          <p>Le build <b>${env.JOB_NAME} #${env.BUILD_NUMBER}</b> est de nouveau vert.</p>
          <p>Commit : ${env.TAG}</p>
          <p><a href="${env.BUILD_URL}">Voir le build</a></p>
        """
      )
    }
  }
}
