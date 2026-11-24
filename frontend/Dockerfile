# Frontend TicketFlow (React) : compilé avec Node.js, servi par nginx sans droits root

# Étape "build" : produire les fichiers statiques (HTML, CSS, JavaScript)
FROM node:22-alpine AS build
WORKDIR /app

# Dépendances d'abord, pour profiter du cache
COPY package*.json ./
RUN npm install

# Puis le code, et la compilation dans le dossier dist/
COPY . .
RUN npm run build

# Étape finale : nginx sert les fichiers et relaie /api vers le backend
FROM nginxinc/nginx-unprivileged:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
