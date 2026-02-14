FROM node:20-alpine

WORKDIR /app

# Copiamos package.json primero
COPY package*.json ./

RUN npm install

# Copiamos el resto
COPY . .

# Exponemos puerto Astro default
EXPOSE 4321

# Ejecutamos en modo dev (con host expuesto para Docker)
CMD ["npm", "run", "dev", "--", "--host"]