# syntax=docker/dockerfile:1.7
# Build context: repository root
ARG NEXT_PUBLIC_API_URL
ARG NEXT_PUBLIC_SUPPLIER_API_URL
ARG NEXT_PUBLIC_GOOGLE_MAPS_API_KEY

FROM node:22-alpine AS deps
WORKDIR /app
COPY src/BFA.Public.UI/package.json src/BFA.Public.UI/package-lock.json ./
RUN npm ci

FROM node:22-alpine AS builder
ARG NEXT_PUBLIC_API_URL
ARG NEXT_PUBLIC_SUPPLIER_API_URL
ARG NEXT_PUBLIC_GOOGLE_MAPS_API_KEY
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY src/BFA.Public.UI/ ./
ENV NEXT_TELEMETRY_DISABLED=1 \
    NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL} \
    NEXT_PUBLIC_SUPPLIER_API_URL=${NEXT_PUBLIC_SUPPLIER_API_URL} \
    NEXT_PUBLIC_GOOGLE_MAPS_API_KEY=${NEXT_PUBLIC_GOOGLE_MAPS_API_KEY}
RUN npm run build

FROM node:22-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production \
    NEXT_TELEMETRY_DISABLED=1 \
    HOSTNAME=0.0.0.0 \
    PORT=3000
EXPOSE 3000
RUN addgroup --system --gid 1001 nodejs \
  && adduser --system --uid 1001 nextjs
COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static
USER nextjs
CMD ["sh", "-c", "node server.js"]
