// Polytype 0.17.0 – https://github.com/fasttime/Polytype
!function () { "use strict"; (t => { try { (class { })() } catch { return } throw Error("Polytype cannot be transpiled to ES5 or earlier code.") })(); const t = Function.prototype, e = Map, o = Object, { create: n, defineProperties: r, defineProperty: s, freeze: c, getOwnPropertyDescriptor: l, getOwnPropertyDescriptors: i, getPrototypeOf: u, setPrototypeOf: a } = o, p = Proxy, f = Reflect, { apply: y, construct: h, get: d, set: w } = f, g = Set, b = String, m = Symbol.hasInstance, v = TypeError, P = { apply(t, e, o) { if (ut(e)) { const [t] = o, n = mt(t) && tt(u(t)); if (n) { const r = G(t, n); e = new p(e, r), delete o[0] } } return k(e, ...o) } }, _ = { setPrototypeOf: () => !1 }, O = { __proto__: _, apply() { throw v("Constructor cannot be invoked without 'new'") } }, S = [], $ = c({ __proto__: null }), x = "result", j = "target", I = { apply(t, e, [o]) { if (mt(o)) { const t = M(e); if (yt(t, o)) return !0 } return !1 } }, q = ["function", "object", "undefined"], D = Symbol.for("Polytype inquiry: prototypes"), E = Symbol.for("Polytype inquiry: this supplier"); let L = t.call, T = t => L.bind(t); const k = T(t.bind), A = T(t[m]), C = T(t.toString), M = T(o.prototype.valueOf); T = null, L = null; const z = (t, e) => { if (t.has(e)) { const t = `Duplicate superclass ${vt(e)}`; throw v(t) } }, F = t => { if (!ut(t)) throw v("Argument is not a function") }, { classes: N } = { classes(...t) { if (!t.length) throw v("No superclasses specified"); const e = new g, o = new g; for (const n of t) { if (z(e, n), !at(n)) { const t = `${vt(n)} is not a constructor`; throw v(t) } const { prototype: t } = n; if (wt(t)) { const t = `Property 'prototype' of ${vt(n)} is not an object or null`; throw v(t) } e.add(n), null !== t && o.add(t) } const s = function (t, e) { const o = V(t), s = B(t), c = U(e), l = R(t), i = W(l, t, O), u = n(null, { constructor: Y(i, !0), class: Y(c) }), a = W(u, e, _), p = { class: Y(o), name: { get: s }, prototype: Y(a) }; return r(l, p), i }(e, o); return function (...t) { const e = new g, o = new g; for (const n of t) for (let t of n) for (; !e.has(t);) { e.add(t); { const { constructor: e } = t; at(e) && lt(e, o) } { const e = u(t); if (null === e) { it(t, "isPrototypeOf", I); break } t = e } } }(e, o), s } }, R = t => { const o = function (...o) { const n = [], s = function () { let t; return { get: () => t, set: e => { t = e } } }(); { const r = function (t, o) { const n = new e; let r, s; const c = t => { if (r === !t) throw v("Mixed argument styles"); r = t }; for (const e of o) { if (bt(e)) throw v("Invalid arguments"); let o, r; if (void 0 !== e && mt(o = e.super)) { if (c(!0), z(n, o), !t.has(o)) { const t = `${vt(o)} is not a direct superclass`; throw v(t) } if (r = e.arguments, bt(r)) { const t = `Invalid arguments for superclass ${vt(o)}`; throw v(t) } } else c(!1), s || (s = t.values()), o = s.next().value, r = e; const l = void 0 !== r ? H(r) : void 0; n.set(o, l) } return n }(t, o), l = function (t, e) { function o() { throw v("Operation not supported") } return delete o.length, delete o.name, o.prototype = J(t, e.prototype), a(o, e), c(o), o }(s.get, new.target); for (const e of t) { const t = r.get(e) ?? S, o = h(e, t, l), s = i(o); n.push(s) } } s.set(this); for (const t of n) r(this, t); for (let t; t = n.pop();)r(this, t) }; return a(o, null), o }, B = t => () => `(${[...t].map((({ name: t }) => b(t)))})`, G = (t, e) => ({ apply: (o, n, r) => (t = e() ?? t, y(o, t, r)) }), H = t.apply.bind(((...t) => t), null), J = (t, e) => { const o = n(e), r = { get: (e, o, n) => (t() || o === E && ht(n, s) && (n[x] = t), d(e, o, n)) }, s = new p(o, r); return s }, K = (t, e) => { const o = { get(o, r) { let s = d(t, r, e); if (ut(s)) { const t = Q(e, n); s = new p(s, t) } return s }, set: (o, n, r) => w(t, n, r, e) }, n = new p($, o); return n }, Q = (t, e) => ({ apply: (o, n, r) => (n === e && (n = t), y(o, n, r)) }), U = t => { const { class: e } = { class(e) { F(e); const { prototype: o } = e; if (!t.has(o)) { const t = mt(o) ? "Property 'prototype' of argument does not match any direct superclass" : "Property 'prototype' of argument is not an object"; throw v(t) } return K(o, this) } }; return e }, V = t => { const { class: e } = { class(e) { if (!t.has(e)) throw F(e), v("Argument is not a direct superclass"); return K(e, this) } }; return e }, W = (t, e, o) => { const n = [t, ...e], r = new p(t, { __proto__: o, get(t, o, s) { o === D && ht(s, r) && (s[x] = e.values()); const c = n.find(Pt(o)); if (void 0 !== c) return d(c, o, s) }, has: (t, e) => n.some(Pt(e)), set(t, e, o, r) { const s = n.find(Pt(e)); return void 0 !== s ? w(s, e, o, r) : (X(r, e, o, !0), !0) } }); return r }, X = (t, e, o, n = !1) => s(t, e, Y(o, !0, n)), Y = (t, e, o) => ({ value: t, writable: e, enumerable: o, configurable: e }), Z = t => { const e = ct(t, D); if (void 0 !== e) { const t = [...e]; for (const e of t) mt(e) || nt(); return t } }, tt = t => { const e = ct(t, E); return void 0 === e || ut(e) || nt(), e }, { getPrototypeListOf: et } = { getPrototypeListOf: t => { let e; { const o = u(t); null !== o ? (e = Z(o), e || (e = [o])) : e = [] } return e } }, ot = t => { let e = Z(t); if (!e) { const o = u(t); e = null !== o ? [o] : S } return e }, nt = () => { throw v("Corrupt inquiry result") }, { [m]: rt } = { [m](t) { st = !0; try { if (ut(this)) { const e = A(this, t); if (!st) return e; if (e || mt(t) && yt(this.prototype, t)) return !0 } return !1 } finally { st = !1 } } }; let st = !1; const ct = (t, e) => { const o = { __proto__: null, [j]: t }; return d(t, e, o), o[x] }, lt = (t, e) => { if (!e.has(t)) { e.add(t); const o = ot(t); let n = !1; for (const t of o) ft(t) ? it(t, "bind", P) : (lt(t, e), n = !0); n || X(t, m, rt) } }, it = (t, e, o) => { const n = l(t, e), r = n?.value; r && dt(r, e) && (n.value = new p(r, o), s(t, e, n)) }, ut = t => "function" == typeof t, at = t => { if (ut(t)) { const e = k(t); X(e, "prototype", null); const o = new p(e, pt); try { return new class extends o { }, !0 } catch { } } return !1 }, pt = { construct() { return this } }, ft = t => { if (dt(t, "")) { const e = l(t, m); if (e && !e.writable && !e.enumerable && !e.configurable && dt(e.value, "[Symbol.hasInstance]")) return !0 } return !1 }, yt = (t, e) => ot(e).some((e => e === t || yt(t, e))), ht = (t, e) => mt(t) && null === u(t) && t !== e && t[j] === e, dt = (t, e) => { let o; try { o = C(t) } catch { return !1 } const n = /^function (.*)\(\) {\s+\[native code]\s}$/.exec(o); return null != n && n[1] === e && !at(t) }, wt = t => void 0 === t || gt(t), gt = t => !q.includes(typeof t), bt = t => null === t || gt(t), mt = t => null !== t && !wt(t), vt = t => { let e; return ut(t) && (({ name: e } = t), null != e && (e = b(e), e)) || (e = b(t)), e }, Pt = t => e => t in e; !globalThis.hasOwnProperty("classes") && (X(globalThis, "classes", N), X(o, "getPrototypeListOf", et)) }();

(function (global) {
    let dotnetJs = global.dotnetJs = {};

    const finalizer = new FinalizationRegistry((_object) => {
        _object.$dtor();
    });

    dotnetJs.$finalizer = function (_this) {
        //TODO: nned a way to map heldValue back to the object being destroyed
        //finalizer.register(myObject, "_");
    }
    dotnetJs.$nomix = class { }

    dotnetJs.$mix = function () {
        var mixed = classes(...arguments);
        return mixed;
    }

    dotnetJs.$boot = function () {
        return dotnetJs;
    }

    dotnetJs.$bind = function (_function, _this) {
        var bound = _function.bind(_this);
        bound.$target = _this;
        return bound
    }

    dotnetJs.$combine = function (_function1, _function2) {
        if (_function1 == null && _function2 == null)
            return null;
        if (_function1 != null && _function1.$functions && _function2 != null && _function2.$functions != null) {  //both multicast delegate
            _function1.$functions.push(..._function2.$functions);
            return _function1;
        }
        if (_function1 != null && _function1.$functions && _function2 != null) { //multicast delegate
            _function1.$functions.push(_function2);
            return _function1;
        }
        if (_function2 != null && _function2.$functions && _function1 != null) { //multicast delegate
            _function2.$functions.push(_function1);
            return _function2;
        }
        if (_function1 == null && _function2 != null)
            return _function2;
        if (_function2 == null && _function1 != null)
            return _function1;
        //both are single delegate
        var functions = [_function1, _function2];
        return { $functions: functions };
    }

    dotnetJs.$remove = function (functions, _function) {
        if (functions == null || _function == null)
            return functions;
        if (functions.$functions) {
            var index = -1;
            for (var i = functions.$functions.length - 1; i >= 0; i--) {
                var f = functions.$functions[i];
                if (f === _function) {
                    index = i;
                    break;
                }
            }
            if (index >= 0) {
                functions.$functions.splice(index, 1);
                if (functions.$functions.length == 1) {
                    return functions.$functions[0];
                }
            }
        } else {
            if (functions === _function)
                return null;
        }
        return functions;
    }

    dotnetJs.$ns = function (fullTypeName, type) {
        var namespace = dotnetJs;
        var builtNameSpace = "";
        var names = fullTypeName.split('.');
        if (fullTypeName.length > 0) {
            for (var i = 0; i < names.length - 1; i++) {
                var n = namespace[names[i]];
                if (builtNameSpace.length > 0)
                    builtNameSpace += ".";
                builtNameSpace += n;
                if (!n) {
                    n = {};
                    namespace[names[i]] = n;
                }
                namespace = n;
            }
        }
        var typeName = names[names.length - 1]
        namespace[typeName] = type;
    }

    dotnetJs.$exp = function (fn) {
        return fn();
    }

    dotnetJs.$default = function (prototype) {
        if (!prototype)
            return null;
        var type = prototype.$type;
        if (prototype.$default)
            return prototype.$default();
        if (type && type.$default)
            return type.$default();
        if (prototype && prototype.IsValueType === false)
            return null;
        if (type && type.IsValueType == false)
            return null;
        if (prototype.$default)
            return prototype.$default();
        if (type && type.$default)
            return type.$default();
        if (prototype.$is && prototype.$is(0)) //test numeric type
            return 0;
        if (type && type.$is && type.$is(0)) //test numeric type
            return 0;
        if (prototype.Zero) //test long and decimal type
            return prototype.Zero;
        if (type && type.Zero) //test long and decimal type
            return type.Zero;
        return new prototype();
    }

    dotnetJs.$box = function (value, valueType) {
    }

    dotnetJs.$ifnn = function (value, whenNotNull) {
        if (value) {
            return whenNotNull(value);
        }
        return null;
    }

    dotnetJs.$discard = {
        get $v() { },
        set $v(v) { }
    }
    dotnetJs.$typeof = function (value) {
        return value.$type;
    }
    dotnetJs.$getType = function (value) {
        if (value == null)
            throw new Error();
        var prototype = Object.getPrototypeOf(value);
        if (prototype.$type)
            return prototype.$type;
        prototype = value.constructor;
        if (prototype.$type)
            return prototype.$type;
    }

    dotnetJs.$is = function (value, type) {
        if (value === null)
            return false;
        if (type.$is && type.$is(value))
            return true;
        if (type.FullName == "System.Object")
            return true;
        if (value.$prototype && value.$prototype.FullName == type.FullName)
            return true;
        if (value instanceof type)
            return true;
        var prototype = Object.getPrototypeOf(value)?.$prototype
        var typePrototype = type.$prototype;
        if (prototype && typePrototype) {

        }
        return false;
    }
    dotnetJs.$as = function (value, type) {
        if (dotnetJs.$is(value, type))
            return value;
        return null;
    }
    dotnetJs.$tryCast = function (value, type) {
        if (dotnetJs.$is(value, type))
            return value;
        if (type.$is && type.$is(value))
            return value;
        return null;
    }
    dotnetJs.$cast = function (value, type) {
        if (value === null)
            return null;
        var val = dotnetJs.$tryCast(value, type);
        if (val === null)
            throw new Error();
        return value;
    }
    dotnetJs.$tupleUnpack = function (fn) {
        return { set $v(tuple) { fn(tuple) } };
    }
    dotnetJs.$typeArray = function (type) {
        return dotnetJs.System.Array$$(type);
    }
    dotnetJs.$typePointer = function (type) {
        return dotnetJs.System.Pointer$$(type);
    }
    dotnetJs.$equals = function (a, b) {
        if (a === b)
            return true;
        return false;
    }
    function stringHashCode(str) {
        let hash = 0;
        if (str.length === 0) {
            return hash;
        }
        for (let i = 0; i < str.length; i++) {
            const char = str.charCodeAt(i);
            hash = ((hash << 5) - hash) + char; // Simple bitwise operation
            hash = hash & hash; // Convert to 32bit integer
        }
        return hash;
    }
    dotnetJs.$getHashCode = function (a) {
        if (typeof (a) == "number")
            return a;
        if (typeof (a) == "boolean")
            return a ? 1 : 0;
        if (a.ToString) {
            var str = a.ToString();
            return stringHashCode(str);
        }
        if (a.$type && a.$type.ToString) {
            var str = a.$type.ToString();
            return stringHashCode(str);
        }
        const jsonString = JSON.stringify(obj);
        return stringHashCode(jsonString);
    }
    let inToString;
    dotnetJs.$toString = function (a, defaultValue) {
        if (defaultValue !== null && a === null)
            return defaultValue;
        if (!inToString) {
            if (a.ToString) {
                inToString = true;
                try {
                    var str = a.ToString();
                    return str;
                } finally {
                    inToString = false;
                }
            }
        }
        if (a.toString) {
            var str = a.toString();
            return str;
        }
        return null;
    }
    let inEquals;
    dotnetJs.$equals = function (a, b, T) {
        if (!inEquals) {
            if (T && a.System$IEquatable$$$Equals) {
                inEquals = true;
                try {
                    var eq = a.System$IEquatable$$$Equals(b);
                    return eq;
                } finally {
                    inEquals = false;
                }
            } else if (T && b.System$IEquatable$$$Equals) {
                inEquals = true;
                try {
                    var eq = b.System$IEquatable$$$Equals(a);
                    return eq;
                } finally {
                    inEquals = false;
                }
            } if (a.Equals) {
                inEquals = true;
                try {
                    var eq = a.Equals(b);
                    return eq;
                } finally {
                    inEquals = false;
                }
            } else if (b.Equals) {
                inEquals = true;
                try {
                    var eq = b.Equals(a);
                    return eq;
                } finally {
                    inEquals = false;
                }
            }
        }
        return a == b;
    }
})(window)
