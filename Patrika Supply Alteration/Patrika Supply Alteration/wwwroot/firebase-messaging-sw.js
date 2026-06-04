// Firebase Cloud Messaging Service Worker
importScripts('https://www.gstatic.com/firebasejs/10.12.0/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.12.0/firebase-messaging-compat.js');

firebase.initializeApp({
    apiKey: "AIzaSyBrwa2xlziDYdRPELj_cSHDZtGf-2bKRgM",
    authDomain: "patrika-supply-alteration.firebaseapp.com",
    projectId: "patrika-supply-alteration",
    storageBucket: "patrika-supply-alteration.firebasestorage.app",
    messagingSenderId: "107205074813",
    appId: "1:107205074813:web:385098229d56cd93f3b614"
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage(function (payload) {
    console.log('Background message received:', payload);
    const notificationTitle = payload.notification.title || 'Supply Alteration';
    const notificationOptions = {
        body: payload.notification.body || '',
        icon: '/images/logo.png'
    };
    self.registration.showNotification(notificationTitle, notificationOptions);
});
