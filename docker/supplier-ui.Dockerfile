# syntax=docker/dockerfile:1.7
# Build context: repository root
#
# Railway: set NEXT_PUBLIC_API_URL as a service variable AND ensure it is
# available at build time (Variables → … → "Build" / Docker Build Args).
# Runtime-only vars will not be inlined into the Next.js client bundle.

ARG NEXT_PUBLIC_API_URL

FROM node:22-alpine AS deps
WORKDIR /app
COPY src/BFA.Supplier.UI/package.json src/BFA.Supplier.UI/package-lock.json ./
RUN npm ci

FROM node:22-alpine AS builder
ARG NEXT_PUBLIC_API_URL
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY src/BFA.Supplier.UI/ ./
ENV NEXT_TELEMETRY_DISABLED=1 \
    NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}
RUN if [ -z "$NEXT_PUBLIC_API_URL" ]; then \
      echo "ERROR: NEXT_PUBLIC_API_URL is empty at build time." >&2; \
      echo "In Railway → supplier-ui → Variables, set NEXT_PUBLIC_API_URL" >&2; \
      echo "and enable it for Build (Docker Build Args), then Redeploy with clear cache." >&2; \
      exit 1; \
    fi \
 && echo "Building supplier-ui with NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL" \
 && printf 'NEXT_PUBLIC_API_URL=%s\n' "$NEXT_PUBLIC_API_URL" > .env.production \
 && npm run build

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
CMD ["node", "server.js"]
