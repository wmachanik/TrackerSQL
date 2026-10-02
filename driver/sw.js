// Offline support for the web copy of the app (the Android app has its files built in).
// vite.config.ts writes this file into the build with FILES (every built file) and BUILD filled in.
// The screen itself: network first, falling back to the saved copy. App files: saved copy first.
// API calls are not touched; the app keeps its own data offline.
const FILES = ["index.html","manifest.json","favicon.svg","icons/icon-192.png","icons/icon-512.png","icons/apple-touch-icon.png","assets/index-BZmz5OQl.js","assets/base-C3ayGsuC.js","assets/native-BaES9jFh.js","assets/web-B1pzc8jO.js","assets/web-UXSciXGb.js","assets/web-uxDC_GWo.js","assets/web-x3Dqt15v.js","assets/index-eYbBMG6r.css"]
const CACHE = 'quaffee-driver-' + "0.5.1-1790907079217"
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
