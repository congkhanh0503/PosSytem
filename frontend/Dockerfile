# ==========================================
# Stage 1: Build Vue 3 Frontend
# ==========================================
FROM node:22-alpine AS build-stage
WORKDIR /app

# Copy package.json và package-lock.json để tận dụng cache
COPY package*.json ./
RUN npm install

# Copy source code và build production
COPY . .
RUN npm run build

# ==========================================
# Stage 2: Nginx Web Server phục vụ SPA & Proxy
# ==========================================
FROM nginx:alpine AS production-stage

# Xóa cấu hình mặc định của Nginx
RUN rm -rf /etc/nginx/conf.d/default.conf

# Copy file cấu hình Nginx tối ưu cho SPA và Proxy API
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy kết quả build từ stage trước
COPY --from=build-stage /app/dist /usr/share/nginx/html

# Expose cổng 80
EXPOSE 80

# Chạy Nginx ở chế độ foreground
CMD ["nginx", "-g", "daemon off;"]
