// Offline support for the web copy of the app (the Android app has its files built in).
// vite.config.ts writes this file into the build with FILES (every built file) and BUILD filled in.
// The screen itself: network first, falling back to the saved copy. App files: saved copy first.
// API calls are not touched; the app keeps its own data offline.
const FILES = ["index.html","manifest.json","favicon.svg","icons/icon-192.png","icons/icon-512.png","icons/apple-touch-icon.png","assets/index-CUEMw3A_.js","assets/base-CCH3jh8s.js","assets/native-DXyRtTOs.js","assets/web-AvoZLUYX.js","assets/web-BpzfqOR1.js","assets/web-CwrLMR4A.js","assets/web-J2NKIuYA.js","assets/index-29OnLf6P.css"]
const CACHE = 'quaffee-driver-' + "0.5.7-1793187321114"
const scope = self.registration.scope

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches
      .open(CACHE)
      .then((cache) => cache.addAll(FILES.map((f) => './' + f)))
      .then(() => self.skipWaiting()),
  )
})

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches
      .keys()
      .then((keys) => Promise.all(keys.filter((k) => k.startsWith('quaffee-driver') && k !== CACHE).map((k) => caches.delete(k))))
      .then(() => self.clients.claim()),
  )
})

self.addEventListener('fetch', (event) => {
  const request = event.request
  if (request.method !== 'GET' || !request.url.startsWith(scope)) return

  if (request.mode === 'navigate') {
    event.respondWith(
      fetch(request)
        .then((response) => {
          if (response.ok) {
            const copy = response.clone()
            caches.open(CACHE).then((cache) => cache.put('./index.html', copy))
          }
          return response
        })
        .catch(() => caches.match('./index.html')),
    )
    return
  }

  event.respondWith(caches.match(request).then((saved) => saved || fetch(request)))
})
