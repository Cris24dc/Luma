const _0x540e16 = _0x3561;
(function (_0x51e848, _0x450bf2) {
  const _0x2d75e5 = _0x3561,
    _0x18fccc = _0x51e848();
  while (!![]) {
    try {
      const _0x437223 =
        (parseInt(_0x2d75e5(0x1a2)) / 0x1) *
          (parseInt(_0x2d75e5(0x198)) / 0x2) +
        (parseInt(_0x2d75e5(0x1ad)) / 0x3) *
          (parseInt(_0x2d75e5(0x19e)) / 0x4) +
        parseInt(_0x2d75e5(0x1a6)) / 0x5 +
        parseInt(_0x2d75e5(0x1a7)) / 0x6 +
        (-parseInt(_0x2d75e5(0x19b)) / 0x7) *
          (parseInt(_0x2d75e5(0x1a5)) / 0x8) +
        (parseInt(_0x2d75e5(0x1ae)) / 0x9) *
          (-parseInt(_0x2d75e5(0x1a4)) / 0xa) +
        (parseInt(_0x2d75e5(0x1a9)) / 0xb) *
          (-parseInt(_0x2d75e5(0x1a1)) / 0xc);
      if (_0x437223 === _0x450bf2) break;
      else _0x18fccc["push"](_0x18fccc["shift"]());
    } catch (_0x3829b4) {
      _0x18fccc["push"](_0x18fccc["shift"]());
    }
  }
})(_0x6f73, 0x647e0);
const slides = document[_0x540e16(0x199)](_0x540e16(0x1a0)),
  dots = document[_0x540e16(0x194)](_0x540e16(0x1af));
function _0x3561(_0x2918c3, _0x191166) {
  const _0x6f7343 = _0x6f73();
  return (
    (_0x3561 = function (_0x356189, _0x4eb088) {
      _0x356189 = _0x356189 - 0x194;
      let _0x142c26 = _0x6f7343[_0x356189];
      return _0x142c26;
    }),
    _0x3561(_0x2918c3, _0x191166)
  );
}
let currentIndex = 0x0;
const totalSlides = slides[_0x540e16(0x19f)]["length"];
function updateSlidePosition() {
  const _0x22f0a7 = _0x540e16;
  (slides[_0x22f0a7(0x19d)]["transform"] =
    _0x22f0a7(0x197) + currentIndex * 0x64 + "%)"),
    dots[_0x22f0a7(0x19a)]((_0x4c44f4) =>
      _0x4c44f4[_0x22f0a7(0x1aa)][_0x22f0a7(0x1ac)](_0x22f0a7(0x196))
    ),
    dots[currentIndex]["classList"]["add"](_0x22f0a7(0x196));
}
function autoSlide() {
  (currentIndex = (currentIndex + 0x1) % totalSlides), updateSlidePosition();
}
let slideInterval = setInterval(autoSlide, 0xbb8);
dots[_0x540e16(0x19a)]((_0x4f218b) => {
  const _0x1f2e33 = _0x540e16;
  _0x4f218b[_0x1f2e33(0x1a8)](_0x1f2e33(0x19c), (_0xb997a) => {
    const _0x528807 = _0x1f2e33;
    clearInterval(slideInterval),
      (currentIndex = parseInt(
        _0xb997a[_0x528807(0x1ab)][_0x528807(0x1a3)](_0x528807(0x195))
      )),
      updateSlidePosition(),
      (slideInterval = setInterval(autoSlide, 0xbb8));
  });
});
function _0x6f73() {
  const _0x3b744c = [
    "57ennLds",
    "9FOOIGn",
    ".dot",
    "querySelectorAll",
    "data-index",
    "active",
    "translateX(-",
    "2342PAwUQQ",
    "getElementById",
    "forEach",
    "542668pAkaYq",
    "click",
    "style",
    "108988aWOpKx",
    "children",
    "slides",
    "14292QFRtmH",
    "541jKhgxI",
    "getAttribute",
    "6020990KiuRwe",
    "24jUSBma",
    "3232630MzKlGd",
    "385824nbVRwY",
    "addEventListener",
    "5687SsjKoi",
    "classList",
    "target",
    "remove",
  ];
  _0x6f73 = function () {
    return _0x3b744c;
  };
  return _0x6f73();
}
