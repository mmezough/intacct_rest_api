import Reveal from 'reveal.js';
import RevealHighlight from 'reveal.js/plugin/highlight/highlight.js';
import 'reveal.js/dist/reveal.css';
import 'reveal.js/dist/theme/black.css';
import 'reveal.js/plugin/highlight/monokai.css';
import './css/sage.css';

Reveal.initialize({
  hash: true,
  transition: 'slide',
  transitionSpeed: 'fast',
  backgroundTransition: 'none',
  width: 1280,
  height: 720,
  margin: 0.04,
  center: false,
  navigationMode: 'default',
  plugins: [RevealHighlight],
});
