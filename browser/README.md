# Three Choices — Browser Edition

This is a self-contained, no-build JavaScript adaptation of the Unity roguelike auto-battler. It uses the original project's artwork via relative paths and can be served as static files; no package installation or server-side code is needed.

## Play locally

From the repository root, start any static-file server and open its `/browser/` path. For example, with Python installed:

```bash
python -m http.server 8000
```

Then visit `http://localhost:8000/browser/`. Opening `index.html` directly may block relative asset loading in some browsers.

## GitHub Pages

The Unity WebGL workflow publishes the Unity game at the site root and adds this static edition plus its required artwork in a separate `browser/` folder. After a successful deployment, open:

```text
https://edvinklaebo.github.io/Three-Choices/browser/
```

Embed the same URL on a portfolio page with an iframe:

```html
<iframe
  src="https://edvinklaebo.github.io/Three-Choices/browser/"
  title="Three Choices browser game"
  width="100%"
  height="760"
  loading="lazy"
  allow="fullscreen"
></iframe>
```

The layout supports narrow embeds down to 255px wide and reflows for mobile. There are no same-origin requirements; the page only needs its relative static assets to remain reachable. The workflow copies only the artwork it uses into the Pages build so its relative asset URLs resolve without replacing the root Unity build.
