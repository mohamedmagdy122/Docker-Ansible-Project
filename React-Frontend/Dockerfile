# Stage 1: Build Stage
FROM node:18 AS frontend-build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Stage 2: Runtime Stage
FROM nginx:alpine
# FIX: Copy the static files from 'dist'
COPY --from=frontend-build /app/dist /usr/share/nginx/html
# FIX: Copy config to conf.d instead of overwriting the main nginx.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
