function _0x2610() {
  const _0x2dfffa = [
    "Failed\x20to\x20update\x20task\x20status.",
    "stringify",
    "preventDefault",
    "152260xqEbZR",
    "1302HBwuXe",
    "input[name=\x22__RequestVerificationToken\x22]",
    "setData",
    "text",
    "target",
    "application/json",
    "38448VEiIIy",
    "Hojgi",
    "data-id",
    "1069580nsgTWQ",
    "20265fswNCQ",
    "3912327wlrksB",
    "querySelector",
    "POST",
    ".dropzone",
    "2673744iCEsGO",
    "catch",
    "appendChild",
    "319qsETGt",
    "/Tasks/UpdateStatus",
    "EMtxi",
    "data-status",
    "dataTransfer",
    "getAttribute",
    "error",
    "then",
    "12jCqSPg",
    "closest",
    "916OUhQmq",
    "getData",
    "3946671PUeCdP",
  ];
  _0x2610 = function () {
    return _0x2dfffa;
  };
  return _0x2610();
}
(function (_0x221947, _0x524f2d) {
  const _0x1b29d4 = _0x3bcb,
    _0xb484be = _0x221947();
  while (!![]) {
    try {
      const _0x4f26b8 =
        (parseInt(_0x1b29d4(0x1c0)) / 0x1) *
          (-parseInt(_0x1b29d4(0x1a5)) / 0x2) +
        -parseInt(_0x1b29d4(0x1a1)) / 0x3 +
        (-parseInt(_0x1b29d4(0x19f)) / 0x4) *
          (-parseInt(_0x1b29d4(0x1b0)) / 0x5) +
        (parseInt(_0x1b29d4(0x1ac)) / 0x6) *
          (-parseInt(_0x1b29d4(0x1a6)) / 0x7) +
        -parseInt(_0x1b29d4(0x1b5)) / 0x8 +
        parseInt(_0x1b29d4(0x1b1)) / 0x9 +
        (-parseInt(_0x1b29d4(0x1af)) / 0xa) *
          (-parseInt(_0x1b29d4(0x1b8)) / 0xb);
      if (_0x4f26b8 === _0x524f2d) break;
      else _0xb484be["push"](_0xb484be["shift"]());
    } catch (_0x51004d) {
      _0xb484be["push"](_0xb484be["shift"]());
    }
  }
})(_0x2610, 0xad317);
function allowDrop(_0x1d6c84) {
  const _0x46dfcd = _0x3bcb;
  _0x1d6c84[_0x46dfcd(0x1a4)]();
}
function _0x3bcb(_0x5ed0a5, _0x212068) {
  const _0x261090 = _0x2610();
  return (
    (_0x3bcb = function (_0x3bcb1f, _0x199da2) {
      _0x3bcb1f = _0x3bcb1f - 0x19e;
      let _0x17c4d0 = _0x261090[_0x3bcb1f];
      return _0x17c4d0;
    }),
    _0x3bcb(_0x5ed0a5, _0x212068)
  );
}
function drag(_0x50018a) {
  const _0x335321 = _0x3bcb;
  _0x50018a[_0x335321(0x1bc)][_0x335321(0x1a8)](
    "text",
    _0x50018a[_0x335321(0x1aa)][_0x335321(0x1bd)](_0x335321(0x1ae))
  );
}
function drop(_0x4fc018) {
  const _0x39dead = _0x3bcb;
  _0x4fc018["preventDefault"]();
  const _0x447a9b = _0x4fc018[_0x39dead(0x1bc)][_0x39dead(0x1a0)](
      _0x39dead(0x1a9)
    ),
    _0x14a2ca = _0x4fc018["target"]
      [_0x39dead(0x19e)](_0x39dead(0x1b4))
      [_0x39dead(0x1bd)](_0x39dead(0x1bb)),
    _0x2815ad = document[_0x39dead(0x1b2)]("#task-card-" + _0x447a9b);
  _0x4fc018[_0x39dead(0x1aa)]
    ["closest"](_0x39dead(0x1b4))
    [_0x39dead(0x1b7)](_0x2815ad),
    updateTaskStatus(_0x447a9b, _0x14a2ca);
}
function updateTaskStatus(_0x319ac7, _0x52e6dc) {
  const _0x4414b8 = _0x3bcb;
  fetch(_0x4414b8(0x1b9), {
    method: _0x4414b8(0x1b3),
    headers: {
      "Content-Type": _0x4414b8(0x1ab),
      RequestVerificationToken: document[_0x4414b8(0x1b2)](_0x4414b8(0x1a7))[
        "value"
      ],
    },
    body: JSON[_0x4414b8(0x1a3)]({ id: _0x319ac7, status: _0x52e6dc }),
  })
    [_0x4414b8(0x1bf)]((_0x1c5954) => {
      const _0x18ca2f = _0x4414b8;
      "NVpnA" === _0x18ca2f(0x1ad)
        ? _0x20ba4b["error"](_0x18ca2f(0x1a2))
        : !_0x1c5954["ok"] &&
          ("WyXjs" === _0x18ca2f(0x1ba)
            ? _0x539138[_0x18ca2f(0x1bc)][_0x18ca2f(0x1a8)](
                _0x18ca2f(0x1a9),
                _0x33d2d8[_0x18ca2f(0x1aa)][_0x18ca2f(0x1bd)](_0x18ca2f(0x1ae))
              )
            : console[_0x18ca2f(0x1be)](
                "Failed\x20to\x20update\x20task\x20status."
              ));
    })
    [_0x4414b8(0x1b6)]((_0x37125c) =>
      console[_0x4414b8(0x1be)]("Error:", _0x37125c)
    );
}
