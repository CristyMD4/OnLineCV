window.cvUi = {
  registerHeaderScroll(dotNetRef) {
    if (window.cvUi._scrollHandler) {
      window.removeEventListener("scroll", window.cvUi._scrollHandler);
    }

    let lastScrollY = window.scrollY;
    let ticking = false;
    let hidden = false;

    const setHidden = (nextHidden) => {
      if (hidden === nextHidden) {
        return;
      }

      hidden = nextHidden;
      dotNetRef.invokeMethodAsync("SetHeaderHidden", hidden);
    };

    const update = () => {
      const currentScrollY = window.scrollY;
      const scrollDelta = currentScrollY - lastScrollY;

      if (currentScrollY <= 24) {
        setHidden(false);
      } else if (scrollDelta > 4) {
        setHidden(true);
      } else if (scrollDelta < -4) {
        setHidden(false);
      }

      lastScrollY = currentScrollY;
      ticking = false;
    };

    window.cvUi._scrollHandler = () => {
      if (!ticking) {
        window.requestAnimationFrame(update);
        ticking = true;
      }
    };

    window.addEventListener("scroll", window.cvUi._scrollHandler, { passive: true });
  },

  unregisterHeaderScroll() {
    if (window.cvUi._scrollHandler) {
      window.removeEventListener("scroll", window.cvUi._scrollHandler);
      window.cvUi._scrollHandler = null;
    }
  },

  print() {
    window.print();
  },

  sessionGet(key) {
    return window.sessionStorage.getItem(key);
  },

  sessionSet(key, value) {
    window.sessionStorage.setItem(key, value);
  },

  sessionRemove(key) {
    window.sessionStorage.removeItem(key);
  },
};
