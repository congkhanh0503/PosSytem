/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        diro: {
          primary: '#4f46e5',
          primaryHover: '#4338ca',
          accent: '#2563eb',
          bg: '#f8fafc',
          card: '#ffffff',
          border: '#e2e8f0',
          text: '#0f172a',
          muted: '#64748b'
        },
        barber: {
          dark: '#f8fafc',
          card: '#ffffff',
          border: '#e2e8f0',
          gold: '#4f46e5',
          goldHover: '#4338ca',
          goldMuted: '#818cf8',
          accent: '#2563eb',
          gray: '#64748b',
          light: '#0f172a'
        }
      }
    },
  },
  plugins: [],
}
