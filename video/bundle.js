!function(n){function e(r){if(t[r])return t[r].exports;var o=t[r]={i:r,l:!1,exports:{}};return n[r].call(o.exports,o,o.exports,e),o.l=!0,o.exports}var t={};e.m=n,e.c=t,e.d=function(n,t,r){e.o(n,t)||Object.defineProperty(n,t,{configurable:!1,enumerable:!0,get:r})},e.n=function(n){var t=n&&n.__esModule?function(){return n.default}:function(){return n};return e.d(t,"a",t),t},e.o=function(n,e){return Object.prototype.hasOwnProperty.call(n,e)},e.p="/",e(e.s=142)}({142:function(n,e,t){"use strict";Object.defineProperty(e,"__esModule",{value:!0});var r=t(143);t.d(e,"options",function(){return r.d}),t.d(e,"app",function(){return r.a}),t.d(e,"button",function(){return r.b}),t.d(e,"renderer",function(){return r.e}),t.d(e,"onPlayVideo",function(){return r.c})},143:function(n,e,t){"use strict";function r(){d.destroy();var n=o.Texture.fromVideo("../img/testVideo.mp4"),e=new o.Sprite(n);e.width=c.width,e.height=c.height,u.stage.addChild(e)}t.d(e,"d",function(){return i}),t.d(e,"a",function(){return u}),t.d(e,"b",function(){return d}),t.d(e,"e",function(){return c}),e.c=r;var o=t(16),i=(t.n(o),{backgroundColor:0,transparent:!0}),u=new o.Application(800,600,i);document.body.appendChild(u.view);var d=(new o.Graphics).beginFill(0,.5).drawRoundedRect(0,0,100,100,10).endFill().beginFill(16777215).moveTo(36,30).lineTo(36,70).lineTo(70,50),c=u.renderer;d.x=(c.width-d.width)/2,d.y=(c.height-d.height)/2,d.interactive=!0,d.buttonMode=!0,u.stage.addChild(d),d.on("pointertap",function(){r()})},16:function(n,e){n.exports=PIXI}});
//# sourceMappingURL=bundle.js.map