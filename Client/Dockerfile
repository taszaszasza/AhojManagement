FROM oven/bun:latest
WORKDIR /app

# Install dependencies
COPY package.json bun.lock* ./
RUN bun install --frozen-lockfile

# Copy source and build
COPY . .
RUN bun run build

EXPOSE 8080

# Serve the static 'dist' directory directly using Bun
CMD ["bun", "x", "serve", "-s", "dist", "-l", "8080"]