var dt = Object.defineProperty;
var lt = (t, e, n) => e in t ? dt(t, e, { enumerable: !0, configurable: !0, writable: !0, value: n }) : t[e] = n;
var m = (t, e, n) => (lt(t, typeof e != "symbol" ? e + "" : e, n), n);
const ft = "http://localhost:5208".replace(/\/+$/, "");
class _t {
  constructor(e = {}) {
    this.configuration = e;
  }
  set config(e) {
    this.configuration = e;
  }
  get basePath() {
    return this.configuration.basePath != null ? this.configuration.basePath : ft;
  }
  get fetchApi() {
    return this.configuration.fetchApi;
  }
  get middleware() {
    return this.configuration.middleware || [];
  }
  get queryParamsStringify() {
    return this.configuration.queryParamsStringify || F;
  }
  get username() {
    return this.configuration.username;
  }
  get password() {
    return this.configuration.password;
  }
  get apiKey() {
    const e = this.configuration.apiKey;
    if (e)
      return typeof e == "function" ? e : () => e;
  }
  get accessToken() {
    const e = this.configuration.accessToken;
    if (e)
      return typeof e == "function" ? e : async () => e;
  }
  get headers() {
    return this.configuration.headers;
  }
  get credentials() {
    return this.configuration.credentials;
  }
}
const pt = new _t();
class h {
  constructor(e = pt) {
    m(this, "middleware");
    m(this, "fetchApi", async (e, n) => {
      let r = { url: e, init: n };
      for (const o of this.middleware)
        o.pre && (r = await o.pre({
          fetch: this.fetchApi,
          ...r
        }) || r);
      let i;
      try {
        i = await (this.configuration.fetchApi || fetch)(r.url, r.init);
      } catch (o) {
        for (const a of this.middleware)
          a.onError && (i = await a.onError({
            fetch: this.fetchApi,
            url: r.url,
            init: r.init,
            error: o,
            response: i ? i.clone() : void 0
          }) || i);
        if (i === void 0)
          throw o instanceof Error ? new Rt(o, "The request failed and the interceptors did not return an alternative response") : o;
      }
      for (const o of this.middleware)
        o.post && (i = await o.post({
          fetch: this.fetchApi,
          url: r.url,
          init: r.init,
          response: i.clone()
        }) || i);
      return i;
    });
    this.configuration = e, this.middleware = e.middleware;
  }
  withMiddleware(...e) {
    const n = this.clone();
    return n.middleware = n.middleware.concat(...e), n;
  }
  withPreMiddleware(...e) {
    const n = e.map((r) => ({ pre: r }));
    return this.withMiddleware(...n);
  }
  withPostMiddleware(...e) {
    const n = e.map((r) => ({ post: r }));
    return this.withMiddleware(...n);
  }
  async request(e, n) {
    const { url: r, init: i } = await this.createFetchParams(e, n), o = await this.fetchApi(r, i);
    if (o.status >= 200 && o.status < 300)
      return o;
    throw new Ot(o, "Response returned an error code");
  }
  async createFetchParams(e, n) {
    let r = this.configuration.basePath + e.path;
    e.query !== void 0 && Object.keys(e.query).length !== 0 && (r += "?" + this.configuration.queryParamsStringify(e.query));
    const i = Object.assign({}, this.configuration.headers, e.headers);
    Object.keys(i).forEach((E) => i[E] === void 0 ? delete i[E] : {});
    const o = typeof n == "function" ? n : async () => n, a = {
      method: e.method,
      headers: i,
      body: e.body,
      credentials: this.configuration.credentials
    }, p = {
      ...a,
      ...await o({
        init: a,
        context: e
      })
    }, ct = {
      ...p,
      body: yt(p.body) || p.body instanceof URLSearchParams || mt(p.body) ? p.body : JSON.stringify(p.body)
    };
    return { url: r, init: ct };
  }
  clone() {
    const e = this.constructor, n = new e(this.configuration);
    return n.middleware = this.middleware.slice(), n;
  }
}
function mt(t) {
  return typeof Blob < "u" && t instanceof Blob;
}
function yt(t) {
  return typeof FormData < "u" && t instanceof FormData;
}
class Ot extends Error {
  constructor(n, r) {
    super(r);
    m(this, "name", "ResponseError");
    this.response = n;
  }
}
class Rt extends Error {
  constructor(n, r) {
    super(r);
    m(this, "name", "FetchError");
    this.cause = n;
  }
}
class u extends Error {
  constructor(n, r) {
    super(r);
    m(this, "name", "RequiredError");
    this.field = n;
  }
}
const On = {
  csv: ",",
  ssv: " ",
  tsv: "	",
  pipes: "|"
};
function s(t, e) {
  const n = t[e];
  return n != null;
}
function F(t, e = "") {
  return Object.keys(t).map((n) => D(n, t[n], e)).filter((n) => n.length > 0).join("&");
}
function D(t, e, n = "") {
  const r = n + (n.length ? `[${t}]` : t);
  if (e instanceof Array) {
    const i = e.map((o) => encodeURIComponent(String(o))).join(`&${encodeURIComponent(r)}=`);
    return `${encodeURIComponent(r)}=${i}`;
  }
  if (e instanceof Set) {
    const i = Array.from(e);
    return D(t, i, n);
  }
  return e instanceof Date ? `${encodeURIComponent(r)}=${encodeURIComponent(e.toISOString())}` : e instanceof Object ? F(e, r) : `${encodeURIComponent(r)}=${encodeURIComponent(String(e))}`;
}
function Rn(t, e) {
  return Object.keys(t).reduce(
    (n, r) => ({ ...n, [r]: e(t[r]) }),
    {}
  );
}
function gn(t) {
  for (const e of t)
    if (e.contentType === "multipart/form-data")
      return !0;
  return !1;
}
class c {
  constructor(e, n = (r) => r) {
    this.raw = e, this.transformer = n;
  }
  async value() {
    return this.transformer(await this.raw.json());
  }
}
class Sn {
  constructor(e) {
    this.raw = e;
  }
  async value() {
  }
}
class Tn {
  constructor(e) {
    this.raw = e;
  }
  async value() {
    return await this.raw.blob();
  }
}
class hn {
  constructor(e) {
    this.raw = e;
  }
  async value() {
    return await this.raw.text();
  }
}
function Nn(t) {
  let e = !0;
  return e = e && "address" in t, e;
}
function gt(t) {
  return St(t);
}
function St(t, e) {
  return t == null ? t : {
    address: t.address
  };
}
function Tt(t) {
  if (t !== void 0)
    return t === null ? null : {
      address: t.address
    };
}
function wn(t) {
  let e = !0;
  return e = e && "key_type" in t, e = e && "key_hex" in t, e;
}
function Jn(t) {
  return b(t);
}
function b(t, e) {
  return t == null ? t : {
    key_type: t.key_type,
    key_hex: t.key_hex
  };
}
function ht(t) {
  if (t !== void 0)
    return t === null ? null : {
      key_type: t.key_type,
      key_hex: t.key_hex
    };
}
function En(t) {
  let e = !0;
  return e = e && "key_type" in t, e = e && "key_hex" in t, e;
}
function Fn(t) {
  return I(t);
}
function I(t, e) {
  return t == null ? t : {
    key_type: t.key_type,
    key_hex: t.key_hex
  };
}
function Nt(t) {
  if (t !== void 0)
    return t === null ? null : {
      key_type: t.key_type,
      key_hex: t.key_hex
    };
}
function Dn(t) {
  return !0;
}
function f(t) {
  return wt(t);
}
function wt(t, e) {
  return t == null ? t : {
    state_version: s(t, "state_version") ? t.state_version : void 0,
    timestamp: s(t, "timestamp") ? t.timestamp === null ? null : new Date(t.timestamp) : void 0,
    epoch: s(t, "epoch") ? t.epoch : void 0,
    round: s(t, "round") ? t.round : void 0
  };
}
function _(t) {
  if (t !== void 0)
    return t === null ? null : {
      state_version: t.state_version,
      timestamp: t.timestamp === void 0 ? void 0 : t.timestamp === null ? null : t.timestamp.toISOString(),
      epoch: t.epoch,
      round: t.round
    };
}
function bn(t) {
  let e = !0;
  return e = e && "address" in t, e;
}
function In(t) {
  return Jt(t);
}
function Jt(t, e) {
  return t == null ? t : {
    address: t.address,
    at_state_identifier: s(t, "at_state_identifier") ? f(t.at_state_identifier) : void 0
  };
}
function Et(t) {
  if (t !== void 0)
    return t === null ? null : {
      address: t.address,
      at_state_identifier: _(t.at_state_identifier)
    };
}
const vn = {
  FungibleResource: "fungible_resource",
  NonFungibleResource: "non_fungible_resource",
  AccountComponent: "account_component"
};
function N(t) {
  return Ft(t);
}
function Ft(t, e) {
  return t;
}
function qn(t) {
  return t;
}
function xn(t) {
  let e = !0;
  return e = e && "discriminator" in t, e;
}
function An(t) {
  return v(t);
}
function v(t, e) {
  return t == null ? t : {
    discriminator: N(t.discriminator)
  };
}
function Dt(t) {
  if (t !== void 0)
    return t === null ? null : {
      discriminator: t.discriminator
    };
}
function Pn(t) {
  let e = !0;
  return e = e && "discriminator" in t, e = e && "total_supply_attos" in t, e = e && "total_minted_attos" in t, e = e && "total_burnt_attos" in t, e;
}
function kn(t) {
  return q(t);
}
function q(t, e) {
  return t == null ? t : {
    discriminator: N(t.discriminator),
    total_supply_attos: t.total_supply_attos,
    total_minted_attos: t.total_minted_attos,
    total_burnt_attos: t.total_burnt_attos
  };
}
function bt(t) {
  if (t !== void 0)
    return t === null ? null : {
      discriminator: t.discriminator,
      total_supply_attos: t.total_supply_attos,
      total_minted_attos: t.total_minted_attos,
      total_burnt_attos: t.total_burnt_attos
    };
}
function Cn(t) {
  return !0;
}
function x(t) {
  return It(t);
}
function It(t, e) {
  return t == null ? t : {
    id_hex: s(t, "id_hex") ? t.id_hex : void 0,
    immutable_data_hex: s(t, "immutable_data_hex") ? t.immutable_data_hex : void 0,
    mutable_data_hex: s(t, "mutable_data_hex") ? t.mutable_data_hex : void 0
  };
}
function A(t) {
  if (t !== void 0)
    return t === null ? null : {
      id_hex: t.id_hex,
      immutable_data_hex: t.immutable_data_hex,
      mutable_data_hex: t.mutable_data_hex
    };
}
function Mn(t) {
  let e = !0;
  return e = e && "total_count" in t, e = e && "items" in t, e;
}
function vt(t) {
  return qt(t);
}
function qt(t, e) {
  return t == null ? t : {
    total_count: t.total_count,
    previous_cursor: s(t, "previous_cursor") ? t.previous_cursor : void 0,
    next_cursor: s(t, "next_cursor") ? t.next_cursor : void 0,
    items: t.items.map(x)
  };
}
function xt(t) {
  if (t !== void 0)
    return t === null ? null : {
      total_count: t.total_count,
      previous_cursor: t.previous_cursor,
      next_cursor: t.next_cursor,
      items: t.items.map(A)
    };
}
function Ln(t) {
  let e = !0;
  return e = e && "discriminator" in t, e = e && "ids" in t, e;
}
function Gn(t) {
  return P(t);
}
function P(t, e) {
  return t == null ? t : {
    discriminator: N(t.discriminator),
    ids: vt(t.ids)
  };
}
function At(t) {
  if (t !== void 0)
    return t === null ? null : {
      discriminator: t.discriminator,
      ids: xt(t.ids)
    };
}
function k(t) {
  return Pt(t);
}
function Pt(t, e) {
  if (t == null)
    return t;
  switch (t.discriminator) {
    case "account_component":
      return { ...v(t), discriminator: "account_component" };
    case "fungible_resource":
      return { ...q(t), discriminator: "fungible_resource" };
    case "non_fungible_resource":
      return { ...P(t), discriminator: "non_fungible_resource" };
    default:
      throw new Error(`No variant of EntityDetailsResponseDetails exists with 'discriminator=${t.discriminator}'`);
  }
}
function C(t) {
  if (t !== void 0) {
    if (t === null)
      return null;
    switch (t.discriminator) {
      case "account_component":
        return Dt(t);
      case "fungible_resource":
        return bt(t);
      case "non_fungible_resource":
        return At(t);
      default:
        throw new Error(`No variant of EntityDetailsResponseDetails exists with 'discriminator=${t.discriminator}'`);
    }
  }
}
function Kn(t) {
  let e = !0;
  return e = e && "key" in t, e = e && "value" in t, e;
}
function w(t) {
  return kt(t);
}
function kt(t, e) {
  return t == null ? t : {
    key: t.key,
    value: t.value
  };
}
function J(t) {
  if (t !== void 0)
    return t === null ? null : {
      key: t.key,
      value: t.value
    };
}
function Un(t) {
  let e = !0;
  return e = e && "total_count" in t, e = e && "items" in t, e;
}
function M(t) {
  return Ct(t);
}
function Ct(t, e) {
  return t == null ? t : {
    total_count: t.total_count,
    previous_cursor: s(t, "previous_cursor") ? t.previous_cursor : void 0,
    next_cursor: s(t, "next_cursor") ? t.next_cursor : void 0,
    items: t.items.map(w)
  };
}
function L(t) {
  if (t !== void 0)
    return t === null ? null : {
      total_count: t.total_count,
      previous_cursor: t.previous_cursor,
      next_cursor: t.next_cursor,
      items: t.items.map(J)
    };
}
function Vn(t) {
  let e = !0;
  return e = e && "network" in t, e = e && "version" in t, e = e && "timestamp" in t, e = e && "epoch" in t, e = e && "round" in t, e;
}
function d(t) {
  return Mt(t);
}
function Mt(t, e) {
  return t == null ? t : {
    network: t.network,
    version: t.version,
    timestamp: t.timestamp,
    epoch: t.epoch,
    round: t.round
  };
}
function l(t) {
  if (t !== void 0)
    return t === null ? null : {
      network: t.network,
      version: t.version,
      timestamp: t.timestamp,
      epoch: t.epoch,
      round: t.round
    };
}
function $n(t) {
  let e = !0;
  return e = e && "ledger_state" in t, e = e && "address" in t, e = e && "metadata" in t, e = e && "details" in t, e;
}
function Lt(t) {
  return Gt(t);
}
function Gt(t, e) {
  return t == null ? t : {
    ledger_state: d(t.ledger_state),
    address: t.address,
    metadata: M(t.metadata),
    details: k(t.details)
  };
}
function zn(t) {
  if (t !== void 0)
    return t === null ? null : {
      ledger_state: l(t.ledger_state),
      address: t.address,
      metadata: L(t.metadata),
      details: C(t.details)
    };
}
function Bn(t) {
  let e = !0;
  return e = e && "address" in t, e = e && "metadata" in t, e = e && "details" in t, e;
}
function Hn(t) {
  return Kt(t);
}
function Kt(t, e) {
  return t == null ? t : {
    address: t.address,
    metadata: M(t.metadata),
    details: k(t.details)
  };
}
function Qn(t) {
  if (t !== void 0)
    return t === null ? null : {
      address: t.address,
      metadata: L(t.metadata),
      details: C(t.details)
    };
}
function Wn(t) {
  let e = !0;
  return e = e && "items" in t, e;
}
function Xn(t) {
  return Ut(t);
}
function Ut(t, e) {
  return t == null ? t : {
    items: t.items.map(w)
  };
}
function Yn(t) {
  if (t !== void 0)
    return t === null ? null : {
      items: t.items.map(J)
    };
}
function Zn(t) {
  let e = !0;
  return e = e && "items" in t, e;
}
function jn(t) {
  return Vt(t);
}
function Vt(t, e) {
  return t == null ? t : {
    items: t.items.map(x)
  };
}
function tr(t) {
  if (t !== void 0)
    return t === null ? null : {
      items: t.items.map(A)
    };
}
function er(t) {
  let e = !0;
  return e = e && "addresses" in t, e;
}
function nr(t) {
  return $t(t);
}
function $t(t, e) {
  return t == null ? t : {
    addresses: t.addresses,
    at_state_identifier: s(t, "at_state_identifier") ? f(t.at_state_identifier) : void 0
  };
}
function zt(t) {
  if (t !== void 0)
    return t === null ? null : {
      addresses: t.addresses,
      at_state_identifier: _(t.at_state_identifier)
    };
}
function rr(t) {
  let e = !0;
  return e = e && "total_count" in t, e = e && "items" in t, e;
}
function Bt(t) {
  return Ht(t);
}
function Ht(t, e) {
  return t == null ? t : {
    total_count: t.total_count,
    previous_cursor: s(t, "previous_cursor") ? t.previous_cursor : void 0,
    next_cursor: s(t, "next_cursor") ? t.next_cursor : void 0,
    items: t.items.map(w)
  };
}
function Qt(t) {
  if (t !== void 0)
    return t === null ? null : {
      total_count: t.total_count,
      previous_cursor: t.previous_cursor,
      next_cursor: t.next_cursor,
      items: t.items.map(J)
    };
}
function ir(t) {
  let e = !0;
  return e = e && "address" in t, e = e && "metadata" in t, e;
}
function G(t) {
  return Wt(t);
}
function Wt(t, e) {
  return t == null ? t : {
    address: t.address,
    metadata: Bt(t.metadata)
  };
}
function K(t) {
  if (t !== void 0)
    return t === null ? null : {
      address: t.address,
      metadata: Qt(t.metadata)
    };
}
function or(t) {
  let e = !0;
  return e = e && "ledger_state" in t, e = e && "entities" in t, e;
}
function Xt(t) {
  return Yt(t);
}
function Yt(t, e) {
  return t == null ? t : {
    ledger_state: d(t.ledger_state),
    entities: t.entities.map(G)
  };
}
function sr(t) {
  if (t !== void 0)
    return t === null ? null : {
      ledger_state: l(t.ledger_state),
      entities: t.entities.map(K)
    };
}
function ar(t) {
  let e = !0;
  return e = e && "entities" in t, e;
}
function ur(t) {
  return Zt(t);
}
function Zt(t, e) {
  return t == null ? t : {
    entities: t.entities.map(G)
  };
}
function cr(t) {
  if (t !== void 0)
    return t === null ? null : {
      entities: t.entities.map(K)
    };
}
function dr(t) {
  let e = !0;
  return e = e && "address" in t, e;
}
function lr(t) {
  return jt(t);
}
function jt(t, e) {
  return t == null ? t : {
    address: t.address,
    at_state_identifier: s(t, "at_state_identifier") ? f(t.at_state_identifier) : void 0
  };
}
function te(t) {
  if (t !== void 0)
    return t === null ? null : {
      address: t.address,
      at_state_identifier: _(t.at_state_identifier)
    };
}
function fr(t) {
  let e = !0;
  return e = e && "address" in t, e = e && "amount_attos" in t, e;
}
function U(t) {
  return ee(t);
}
function ee(t, e) {
  return t == null ? t : {
    address: t.address,
    amount_attos: t.amount_attos
  };
}
function V(t) {
  if (t !== void 0)
    return t === null ? null : {
      address: t.address,
      amount_attos: t.amount_attos
    };
}
function _r(t) {
  let e = !0;
  return e = e && "total_count" in t, e = e && "items" in t, e;
}
function $(t) {
  return ne(t);
}
function ne(t, e) {
  return t == null ? t : {
    total_count: t.total_count,
    previous_cursor: s(t, "previous_cursor") ? t.previous_cursor : void 0,
    next_cursor: s(t, "next_cursor") ? t.next_cursor : void 0,
    items: t.items.map(U)
  };
}
function z(t) {
  if (t !== void 0)
    return t === null ? null : {
      total_count: t.total_count,
      previous_cursor: t.previous_cursor,
      next_cursor: t.next_cursor,
      items: t.items.map(V)
    };
}
function pr(t) {
  let e = !0;
  return e = e && "address" in t, e = e && "amount" in t, e;
}
function B(t) {
  return re(t);
}
function re(t, e) {
  return t == null ? t : {
    address: t.address,
    amount: t.amount
  };
}
function H(t) {
  if (t !== void 0)
    return t === null ? null : {
      address: t.address,
      amount: t.amount
    };
}
function mr(t) {
  let e = !0;
  return e = e && "total_count" in t, e = e && "items" in t, e;
}
function Q(t) {
  return ie(t);
}
function ie(t, e) {
  return t == null ? t : {
    total_count: t.total_count,
    previous_cursor: s(t, "previous_cursor") ? t.previous_cursor : void 0,
    next_cursor: s(t, "next_cursor") ? t.next_cursor : void 0,
    items: t.items.map(B)
  };
}
function W(t) {
  if (t !== void 0)
    return t === null ? null : {
      total_count: t.total_count,
      previous_cursor: t.previous_cursor,
      next_cursor: t.next_cursor,
      items: t.items.map(H)
    };
}
function yr(t) {
  let e = !0;
  return e = e && "ledger_state" in t, e = e && "address" in t, e = e && "fungible_resources" in t, e = e && "non_fungible_resources" in t, e;
}
function oe(t) {
  return se(t);
}
function se(t, e) {
  return t == null ? t : {
    ledger_state: d(t.ledger_state),
    address: t.address,
    fungible_resources: $(t.fungible_resources),
    non_fungible_resources: Q(t.non_fungible_resources)
  };
}
function Or(t) {
  if (t !== void 0)
    return t === null ? null : {
      ledger_state: l(t.ledger_state),
      address: t.address,
      fungible_resources: z(t.fungible_resources),
      non_fungible_resources: W(t.non_fungible_resources)
    };
}
function Rr(t) {
  let e = !0;
  return e = e && "address" in t, e = e && "fungible_resources" in t, e = e && "non_fungible_resources" in t, e;
}
function gr(t) {
  return ae(t);
}
function ae(t, e) {
  return t == null ? t : {
    address: t.address,
    fungible_resources: $(t.fungible_resources),
    non_fungible_resources: Q(t.non_fungible_resources)
  };
}
function Sr(t) {
  if (t !== void 0)
    return t === null ? null : {
      address: t.address,
      fungible_resources: z(t.fungible_resources),
      non_fungible_resources: W(t.non_fungible_resources)
    };
}
function Tr(t) {
  let e = !0;
  return e = e && "items" in t, e;
}
function hr(t) {
  return ue(t);
}
function ue(t, e) {
  return t == null ? t : {
    items: t.items.map(U)
  };
}
function Nr(t) {
  if (t !== void 0)
    return t === null ? null : {
      items: t.items.map(V)
    };
}
function wr(t) {
  let e = !0;
  return e = e && "items" in t, e;
}
function Jr(t) {
  return ce(t);
}
function ce(t, e) {
  return t == null ? t : {
    items: t.items.map(B)
  };
}
function Er(t) {
  if (t !== void 0)
    return t === null ? null : {
      items: t.items.map(H)
    };
}
function Fr(t) {
  let e = !0;
  return e = e && "type" in t, e;
}
function de(t) {
  return y(t, !1);
}
function y(t, e) {
  if (t == null)
    return t;
  if (!e) {
    if (t.type === "InternalServerError")
      return tt(t, !0);
    if (t.type === "InvalidRequestError")
      return rt(t, !0);
    if (t.type === "InvalidTransactionError")
      return it(t);
    if (t.type === "NotSyncedUpError")
      return ot(t, !0);
    if (t.type === "TransactionNotFoundError")
      return ut(t, !0);
  }
  return {
    type: t.type
  };
}
function O(t) {
  if (t !== void 0)
    return t === null ? null : {
      type: t.type
    };
}
function Dr(t) {
  let e = !0;
  return e = e && "code" in t, e = e && "message" in t, e;
}
function br(t) {
  return le(t);
}
function le(t, e) {
  return t == null ? t : {
    code: t.code,
    message: t.message,
    details: s(t, "details") ? de(t.details) : void 0,
    trace_id: s(t, "trace_id") ? t.trace_id : void 0
  };
}
function Ir(t) {
  if (t !== void 0)
    return t === null ? null : {
      code: t.code,
      message: t.message,
      details: O(t.details),
      trace_id: t.trace_id
    };
}
function vr(t) {
  let e = !0;
  return e = e && "version" in t, e = e && "open_api_schema_version" in t, e;
}
function X(t) {
  return fe(t);
}
function fe(t, e) {
  return t == null ? t : {
    version: t.version,
    open_api_schema_version: t.open_api_schema_version
  };
}
function Y(t) {
  if (t !== void 0)
    return t === null ? null : {
      version: t.version,
      open_api_schema_version: t.open_api_schema_version
    };
}
function qr(t) {
  let e = !0;
  return e = e && "version" in t, e;
}
function Z(t) {
  return _e(t);
}
function _e(t, e) {
  return t == null ? t : {
    version: t.version
  };
}
function j(t) {
  if (t !== void 0)
    return t === null ? null : {
      version: t.version
    };
}
function xr(t) {
  let e = !0;
  return e = e && "ledger_state" in t, e = e && "gateway_api" in t, e = e && "target_ledger_state" in t, e;
}
function pe(t) {
  return me(t);
}
function me(t, e) {
  return t == null ? t : {
    ledger_state: d(t.ledger_state),
    gateway_api: X(t.gateway_api),
    target_ledger_state: Z(t.target_ledger_state)
  };
}
function Ar(t) {
  if (t !== void 0)
    return t === null ? null : {
      ledger_state: l(t.ledger_state),
      gateway_api: Y(t.gateway_api),
      target_ledger_state: j(t.target_ledger_state)
    };
}
function Pr(t) {
  let e = !0;
  return e = e && "gateway_api" in t, e = e && "target_ledger_state" in t, e;
}
function kr(t) {
  return ye(t);
}
function ye(t, e) {
  return t == null ? t : {
    gateway_api: X(t.gateway_api),
    target_ledger_state: Z(t.target_ledger_state)
  };
}
function Cr(t) {
  if (t !== void 0)
    return t === null ? null : {
      gateway_api: Y(t.gateway_api),
      target_ledger_state: j(t.target_ledger_state)
    };
}
function Mr(t) {
  let e = !0;
  return e = e && "exception" in t, e = e && "cause" in t, e;
}
function Lr(t) {
  return tt(t, !1);
}
function tt(t, e) {
  return t == null ? t : {
    ...y(t, e),
    exception: t.exception,
    cause: t.cause
  };
}
function Gr(t) {
  if (t !== void 0)
    return t === null ? null : {
      ...O(t),
      exception: t.exception,
      cause: t.cause
    };
}
function Kr(t) {
  let e = !0;
  return e = e && "exception" in t, e = e && "cause" in t, e;
}
function Ur(t) {
  return Oe(t);
}
function Oe(t, e) {
  return t == null ? t : {
    exception: t.exception,
    cause: t.cause
  };
}
function Vr(t) {
  if (t !== void 0)
    return t === null ? null : {
      exception: t.exception,
      cause: t.cause
    };
}
function $r(t) {
  let e = !0;
  return e = e && "path" in t, e = e && "errors" in t, e;
}
function et(t) {
  return Re(t);
}
function Re(t, e) {
  return t == null ? t : {
    path: t.path,
    errors: t.errors
  };
}
function nt(t) {
  if (t !== void 0)
    return t === null ? null : {
      path: t.path,
      errors: t.errors
    };
}
function zr(t) {
  let e = !0;
  return e = e && "validation_errors" in t, e;
}
function Br(t) {
  return rt(t, !1);
}
function rt(t, e) {
  return t == null ? t : {
    ...y(t, e),
    validation_errors: t.validation_errors.map(et)
  };
}
function Hr(t) {
  if (t !== void 0)
    return t === null ? null : {
      ...O(t),
      validation_errors: t.validation_errors.map(nt)
    };
}
function Qr(t) {
  let e = !0;
  return e = e && "validation_errors" in t, e;
}
function Wr(t) {
  return ge(t);
}
function ge(t, e) {
  return t == null ? t : {
    validation_errors: t.validation_errors.map(et)
  };
}
function Xr(t) {
  if (t !== void 0)
    return t === null ? null : {
      validation_errors: t.validation_errors.map(nt)
    };
}
function Yr(t) {
  return !0;
}
function Zr(t) {
  return it(t);
}
function it(t, e) {
  return t;
}
function jr(t) {
  return t;
}
function ti(t) {
  let e = !0;
  return e = e && "ledger_state" in t, e;
}
function ei(t) {
  return Se(t);
}
function Se(t, e) {
  return t == null ? t : {
    ledger_state: d(t.ledger_state)
  };
}
function ni(t) {
  if (t !== void 0)
    return t === null ? null : {
      ledger_state: l(t.ledger_state)
    };
}
function ri(t) {
  let e = !0;
  return e = e && "request_type" in t, e = e && "current_sync_delay_seconds" in t, e = e && "max_allowed_sync_delay_seconds" in t, e;
}
function ii(t) {
  return ot(t, !1);
}
function ot(t, e) {
  return t == null ? t : {
    ...y(t, e),
    request_type: t.request_type,
    current_sync_delay_seconds: t.current_sync_delay_seconds,
    max_allowed_sync_delay_seconds: t.max_allowed_sync_delay_seconds
  };
}
function oi(t) {
  if (t !== void 0)
    return t === null ? null : {
      ...O(t),
      request_type: t.request_type,
      current_sync_delay_seconds: t.current_sync_delay_seconds,
      max_allowed_sync_delay_seconds: t.max_allowed_sync_delay_seconds
    };
}
function si(t) {
  let e = !0;
  return e = e && "request_type" in t, e = e && "current_sync_delay_seconds" in t, e = e && "max_allowed_sync_delay_seconds" in t, e;
}
function ai(t) {
  return Te(t);
}
function Te(t, e) {
  return t == null ? t : {
    request_type: t.request_type,
    current_sync_delay_seconds: t.current_sync_delay_seconds,
    max_allowed_sync_delay_seconds: t.max_allowed_sync_delay_seconds
  };
}
function ui(t) {
  if (t !== void 0)
    return t === null ? null : {
      request_type: t.request_type,
      current_sync_delay_seconds: t.current_sync_delay_seconds,
      max_allowed_sync_delay_seconds: t.max_allowed_sync_delay_seconds
    };
}
function he(t) {
  return Ne(t);
}
function Ne(t, e) {
  if (t == null)
    return t;
  switch (t.key_type) {
    case "EcdsaSecp256k1":
      return { ...b(t), key_type: "EcdsaSecp256k1" };
    case "EddsaEd25519":
      return { ...I(t), key_type: "EddsaEd25519" };
    default:
      throw new Error(`No variant of PublicKey exists with 'key_type=${t.key_type}'`);
  }
}
function we(t) {
  if (t !== void 0) {
    if (t === null)
      return null;
    switch (t.key_type) {
      case "EcdsaSecp256k1":
        return ht(t);
      case "EddsaEd25519":
        return Nt(t);
      default:
        throw new Error(`No variant of PublicKey exists with 'key_type=${t.key_type}'`);
    }
  }
}
const ci = {
  EcdsaSecp256k1: "EcdsaSecp256k1",
  EddsaEd25519: "EddsaEd25519"
};
function di(t) {
  return Je(t);
}
function Je(t, e) {
  return t;
}
function li(t) {
  return t;
}
function fi(t) {
  return !0;
}
function _i(t) {
  return Ee(t);
}
function Ee(t, e) {
  return t == null ? t : {
    at_state_identifier: s(t, "at_state_identifier") ? f(t.at_state_identifier) : void 0,
    from_state_identifier: s(t, "from_state_identifier") ? f(t.from_state_identifier) : void 0,
    cursor: s(t, "cursor") ? t.cursor : void 0,
    limit: s(t, "limit") ? t.limit : void 0
  };
}
function Fe(t) {
  if (t !== void 0)
    return t === null ? null : {
      at_state_identifier: _(t.at_state_identifier),
      from_state_identifier: _(t.from_state_identifier),
      cursor: t.cursor,
      limit: t.limit
    };
}
function pi(t) {
  let e = !0;
  return e = e && "rri" in t, e;
}
function De(t) {
  return be(t);
}
function be(t, e) {
  return t == null ? t : {
    rri: t.rri
  };
}
function Ie(t) {
  if (t !== void 0)
    return t === null ? null : {
      rri: t.rri
    };
}
function mi(t) {
  let e = !0;
  return e = e && "value" in t, e = e && "token_identifier" in t, e;
}
function ve(t) {
  return qe(t);
}
function qe(t, e) {
  return t == null ? t : {
    value: t.value,
    token_identifier: De(t.token_identifier)
  };
}
function xe(t) {
  if (t !== void 0)
    return t === null ? null : {
      value: t.value,
      token_identifier: Ie(t.token_identifier)
    };
}
const yi = {
  Succeeded: "succeeded",
  Failed: "failed",
  Rejected: "rejected",
  Pending: "pending"
};
function Oi(t) {
  let e = !0;
  return e = e && "status" in t, e;
}
function Ae(t) {
  return Pe(t);
}
function Pe(t, e) {
  return t == null ? t : {
    state_version: s(t, "state_version") ? t.state_version : void 0,
    status: t.status,
    confirmed_time: s(t, "confirmed_time") ? t.confirmed_time === null ? null : new Date(t.confirmed_time) : void 0
  };
}
function ke(t) {
  if (t !== void 0)
    return t === null ? null : {
      state_version: t.state_version,
      status: t.status,
      confirmed_time: t.confirmed_time === void 0 ? void 0 : t.confirmed_time === null ? null : t.confirmed_time.toISOString()
    };
}
function Ri(t) {
  let e = !0;
  return e = e && "transaction_status" in t, e = e && "payload_hash_hex" in t, e = e && "intent_hash_hex" in t, e = e && "transaction_accumulator_hex" in t, e = e && "fee_paid" in t, e;
}
function R(t) {
  return Ce(t);
}
function Ce(t, e) {
  return t == null ? t : {
    transaction_status: Ae(t.transaction_status),
    payload_hash_hex: t.payload_hash_hex,
    intent_hash_hex: t.intent_hash_hex,
    transaction_accumulator_hex: t.transaction_accumulator_hex,
    fee_paid: ve(t.fee_paid)
  };
}
function g(t) {
  if (t !== void 0)
    return t === null ? null : {
      transaction_status: ke(t.transaction_status),
      payload_hash_hex: t.payload_hash_hex,
      intent_hash_hex: t.intent_hash_hex,
      transaction_accumulator_hex: t.transaction_accumulator_hex,
      fee_paid: xe(t.fee_paid)
    };
}
function gi(t) {
  let e = !0;
  return e = e && "ledger_state" in t, e = e && "total_count" in t, e = e && "items" in t, e;
}
function Me(t) {
  return Le(t);
}
function Le(t, e) {
  return t == null ? t : {
    ledger_state: d(t.ledger_state),
    total_count: t.total_count,
    previous_cursor: s(t, "previous_cursor") ? t.previous_cursor : void 0,
    next_cursor: s(t, "next_cursor") ? t.next_cursor : void 0,
    items: t.items.map(R)
  };
}
function Si(t) {
  if (t !== void 0)
    return t === null ? null : {
      ledger_state: l(t.ledger_state),
      total_count: t.total_count,
      previous_cursor: t.previous_cursor,
      next_cursor: t.next_cursor,
      items: t.items.map(g)
    };
}
function Ti(t) {
  let e = !0;
  return e = e && "total_count" in t, e;
}
function hi(t) {
  return Ge(t);
}
function Ge(t, e) {
  return t == null ? t : {
    total_count: t.total_count,
    previous_cursor: s(t, "previous_cursor") ? t.previous_cursor : void 0,
    next_cursor: s(t, "next_cursor") ? t.next_cursor : void 0
  };
}
function Ni(t) {
  if (t !== void 0)
    return t === null ? null : {
      total_count: t.total_count,
      previous_cursor: t.previous_cursor,
      next_cursor: t.next_cursor
    };
}
function wi(t) {
  let e = !0;
  return e = e && "name" in t, e = e && "description" in t, e = e && "icon_url" in t, e = e && "url" in t, e = e && "symbol" in t, e = e && "is_supply_mutable" in t, e = e && "granularity" in t, e;
}
function Ji(t) {
  return Ke(t);
}
function Ke(t, e) {
  return t == null ? t : {
    name: t.name,
    description: t.description,
    icon_url: t.icon_url,
    url: t.url,
    symbol: t.symbol,
    is_supply_mutable: t.is_supply_mutable,
    granularity: t.granularity,
    owner: s(t, "owner") ? gt(t.owner) : void 0
  };
}
function Ei(t) {
  if (t !== void 0)
    return t === null ? null : {
      name: t.name,
      description: t.description,
      icon_url: t.icon_url,
      url: t.url,
      symbol: t.symbol,
      is_supply_mutable: t.is_supply_mutable,
      granularity: t.granularity,
      owner: Tt(t.owner)
    };
}
function Fi(t) {
  let e = !0;
  return e = e && "raw_hex" in t, e = e && "referenced_global_entities" in t, e;
}
function st(t) {
  return Ue(t);
}
function Ue(t, e) {
  return t == null ? t : {
    raw_hex: t.raw_hex,
    referenced_global_entities: t.referenced_global_entities,
    message_hex: s(t, "message_hex") ? t.message_hex : void 0
  };
}
function at(t) {
  if (t !== void 0)
    return t === null ? null : {
      raw_hex: t.raw_hex,
      referenced_global_entities: t.referenced_global_entities,
      message_hex: t.message_hex
    };
}
const Di = {
  Intent: "intent",
  SignedIntent: "signed_intent",
  Notarized: "notarized",
  Payload: "payload"
};
function Ve(t) {
  return $e(t);
}
function $e(t, e) {
  return t;
}
function bi(t) {
  return t;
}
function Ii(t) {
  let e = !0;
  return e = e && "origin" in t, e = e && "value_hex" in t, e;
}
function S(t) {
  return ze(t);
}
function ze(t, e) {
  return t == null ? t : {
    origin: Ve(t.origin),
    value_hex: t.value_hex
  };
}
function T(t) {
  if (t !== void 0)
    return t === null ? null : {
      origin: t.origin,
      value_hex: t.value_hex
    };
}
function vi(t) {
  let e = !0;
  return e = e && "transaction_identifier" in t, e;
}
function qi(t) {
  return Be(t);
}
function Be(t, e) {
  return t == null ? t : {
    transaction_identifier: S(t.transaction_identifier),
    at_state_identifier: s(t, "at_state_identifier") ? f(t.at_state_identifier) : void 0
  };
}
function He(t) {
  if (t !== void 0)
    return t === null ? null : {
      transaction_identifier: T(t.transaction_identifier),
      at_state_identifier: _(t.at_state_identifier)
    };
}
function xi(t) {
  let e = !0;
  return e = e && "ledger_state" in t, e = e && "transaction" in t, e = e && "details" in t, e;
}
function Qe(t) {
  return We(t);
}
function We(t, e) {
  return t == null ? t : {
    ledger_state: d(t.ledger_state),
    transaction: R(t.transaction),
    details: st(t.details)
  };
}
function Ai(t) {
  if (t !== void 0)
    return t === null ? null : {
      ledger_state: l(t.ledger_state),
      transaction: g(t.transaction),
      details: at(t.details)
    };
}
function Pi(t) {
  let e = !0;
  return e = e && "transaction" in t, e = e && "details" in t, e;
}
function ki(t) {
  return Xe(t);
}
function Xe(t, e) {
  return t == null ? t : {
    transaction: R(t.transaction),
    details: st(t.details)
  };
}
function Ci(t) {
  if (t !== void 0)
    return t === null ? null : {
      transaction: g(t.transaction),
      details: at(t.details)
    };
}
function Mi(t) {
  let e = !0;
  return e = e && "transaction_not_found" in t, e;
}
function Li(t) {
  return ut(t, !1);
}
function ut(t, e) {
  return t == null ? t : {
    ...y(t, e),
    transaction_not_found: S(t.transaction_not_found)
  };
}
function Gi(t) {
  if (t !== void 0)
    return t === null ? null : {
      ...O(t),
      transaction_not_found: T(t.transaction_not_found)
    };
}
function Ki(t) {
  let e = !0;
  return e = e && "transaction_not_found" in t, e;
}
function Ui(t) {
  return Ye(t);
}
function Ye(t, e) {
  return t == null ? t : {
    transaction_not_found: S(t.transaction_not_found)
  };
}
function Vi(t) {
  if (t !== void 0)
    return t === null ? null : {
      transaction_not_found: T(t.transaction_not_found)
    };
}
function $i(t) {
  let e = !0;
  return e = e && "unlimited_loan" in t, e;
}
function Ze(t) {
  return je(t);
}
function je(t, e) {
  return t == null ? t : {
    unlimited_loan: t.unlimited_loan
  };
}
function tn(t) {
  if (t !== void 0)
    return t === null ? null : {
      unlimited_loan: t.unlimited_loan
    };
}
function zi(t) {
  let e = !0;
  return e = e && "manifest" in t, e = e && "cost_unit_limit" in t, e = e && "tip_percentage" in t, e = e && "nonce" in t, e = e && "signer_public_keys" in t, e = e && "flags" in t, e;
}
function Bi(t) {
  return en(t);
}
function en(t, e) {
  return t == null ? t : {
    manifest: t.manifest,
    blobs_hex: s(t, "blobs_hex") ? t.blobs_hex : void 0,
    cost_unit_limit: t.cost_unit_limit,
    tip_percentage: t.tip_percentage,
    nonce: t.nonce,
    signer_public_keys: t.signer_public_keys.map(he),
    flags: Ze(t.flags)
  };
}
function nn(t) {
  if (t !== void 0)
    return t === null ? null : {
      manifest: t.manifest,
      blobs_hex: t.blobs_hex,
      cost_unit_limit: t.cost_unit_limit,
      tip_percentage: t.tip_percentage,
      nonce: t.nonce,
      signer_public_keys: t.signer_public_keys.map(we),
      flags: tn(t.flags)
    };
}
function Hi(t) {
  let e = !0;
  return e = e && "core_api_response" in t, e;
}
function rn(t) {
  return on(t);
}
function on(t, e) {
  return t == null ? t : {
    core_api_response: t.core_api_response
  };
}
function Qi(t) {
  if (t !== void 0)
    return t === null ? null : {
      core_api_response: t.core_api_response
    };
}
function Wi(t) {
  let e = !0;
  return e = e && "transaction_identifier" in t, e;
}
function Xi(t) {
  return sn(t);
}
function sn(t, e) {
  return t == null ? t : {
    transaction_identifier: S(t.transaction_identifier),
    at_state_identifier: s(t, "at_state_identifier") ? f(t.at_state_identifier) : void 0
  };
}
function an(t) {
  if (t !== void 0)
    return t === null ? null : {
      transaction_identifier: T(t.transaction_identifier),
      at_state_identifier: _(t.at_state_identifier)
    };
}
function Yi(t) {
  let e = !0;
  return e = e && "ledger_state" in t, e = e && "transaction" in t, e;
}
function un(t) {
  return cn(t);
}
function cn(t, e) {
  return t == null ? t : {
    ledger_state: d(t.ledger_state),
    transaction: R(t.transaction)
  };
}
function Zi(t) {
  if (t !== void 0)
    return t === null ? null : {
      ledger_state: l(t.ledger_state),
      transaction: g(t.transaction)
    };
}
function ji(t) {
  let e = !0;
  return e = e && "transaction" in t, e;
}
function to(t) {
  return dn(t);
}
function dn(t, e) {
  return t == null ? t : {
    transaction: R(t.transaction)
  };
}
function eo(t) {
  if (t !== void 0)
    return t === null ? null : {
      transaction: g(t.transaction)
    };
}
function no(t) {
  let e = !0;
  return e = e && "notarized_transaction" in t, e;
}
function ro(t) {
  return ln(t);
}
function ln(t, e) {
  return t == null ? t : {
    notarized_transaction: t.notarized_transaction
  };
}
function fn(t) {
  if (t !== void 0)
    return t === null ? null : {
      notarized_transaction: t.notarized_transaction
    };
}
function io(t) {
  let e = !0;
  return e = e && "duplicate" in t, e;
}
function _n(t) {
  return pn(t);
}
function pn(t, e) {
  return t == null ? t : {
    duplicate: t.duplicate
  };
}
function oo(t) {
  if (t !== void 0)
    return t === null ? null : {
      duplicate: t.duplicate
    };
}
function so(t) {
  let e = !0;
  return e = e && "address" in t, e;
}
function ao(t) {
  return mn(t);
}
function mn(t, e) {
  return t == null ? t : {
    address: t.address
  };
}
function uo(t) {
  if (t !== void 0)
    return t === null ? null : {
      address: t.address
    };
}
class co extends h {
  async entityDetailsRaw(e, n) {
    if (e.entityDetailsRequest === null || e.entityDetailsRequest === void 0)
      throw new u("entityDetailsRequest", "Required parameter requestParameters.entityDetailsRequest was null or undefined when calling entityDetails.");
    const r = {}, i = {};
    i["Content-Type"] = "application/json";
    const o = await this.request({
      path: "/entity/details",
      method: "POST",
      headers: i,
      query: r,
      body: Et(e.entityDetailsRequest)
    }, n);
    return new c(o, (a) => Lt(a));
  }
  async entityDetails(e, n) {
    return await (await this.entityDetailsRaw(e, n)).value();
  }
  async entityOverviewRaw(e, n) {
    if (e.entityOverviewRequest === null || e.entityOverviewRequest === void 0)
      throw new u("entityOverviewRequest", "Required parameter requestParameters.entityOverviewRequest was null or undefined when calling entityOverview.");
    const r = {}, i = {};
    i["Content-Type"] = "application/json";
    const o = await this.request({
      path: "/entity/overview",
      method: "POST",
      headers: i,
      query: r,
      body: zt(e.entityOverviewRequest)
    }, n);
    return new c(o, (a) => Xt(a));
  }
  async entityOverview(e, n) {
    return await (await this.entityOverviewRaw(e, n)).value();
  }
  async entityResourcesRaw(e, n) {
    if (e.entityResourcesRequest === null || e.entityResourcesRequest === void 0)
      throw new u("entityResourcesRequest", "Required parameter requestParameters.entityResourcesRequest was null or undefined when calling entityResources.");
    const r = {}, i = {};
    i["Content-Type"] = "application/json";
    const o = await this.request({
      path: "/entity/resources",
      method: "POST",
      headers: i,
      query: r,
      body: te(e.entityResourcesRequest)
    }, n);
    return new c(o, (a) => oe(a));
  }
  async entityResources(e, n) {
    return await (await this.entityResourcesRaw(e, n)).value();
  }
}
class lo extends h {
  async gatewayInfoRaw(e, n) {
    if (e.body === null || e.body === void 0)
      throw new u("body", "Required parameter requestParameters.body was null or undefined when calling gatewayInfo.");
    const r = {}, i = {};
    i["Content-Type"] = "application/json";
    const o = await this.request({
      path: "/gateway",
      method: "POST",
      headers: i,
      query: r,
      body: e.body
    }, n);
    return new c(o, (a) => pe(a));
  }
  async gatewayInfo(e, n) {
    return await (await this.gatewayInfoRaw(e, n)).value();
  }
}
class fo extends h {
  async previewTransactionRaw(e, n) {
    if (e.transactionPreviewRequest === null || e.transactionPreviewRequest === void 0)
      throw new u("transactionPreviewRequest", "Required parameter requestParameters.transactionPreviewRequest was null or undefined when calling previewTransaction.");
    const r = {}, i = {};
    i["Content-Type"] = "application/json";
    const o = await this.request({
      path: "/transaction/preview",
      method: "POST",
      headers: i,
      query: r,
      body: nn(e.transactionPreviewRequest)
    }, n);
    return new c(o, (a) => rn(a));
  }
  async previewTransaction(e, n) {
    return await (await this.previewTransactionRaw(e, n)).value();
  }
  async recentTransactionsRaw(e, n) {
    if (e.recentTransactionsRequest === null || e.recentTransactionsRequest === void 0)
      throw new u("recentTransactionsRequest", "Required parameter requestParameters.recentTransactionsRequest was null or undefined when calling recentTransactions.");
    const r = {}, i = {};
    i["Content-Type"] = "application/json";
    const o = await this.request({
      path: "/transaction/recent",
      method: "POST",
      headers: i,
      query: r,
      body: Fe(e.recentTransactionsRequest)
    }, n);
    return new c(o, (a) => Me(a));
  }
  async recentTransactions(e, n) {
    return await (await this.recentTransactionsRaw(e, n)).value();
  }
  async submitTransactionRaw(e, n) {
    if (e.transactionSubmitRequest === null || e.transactionSubmitRequest === void 0)
      throw new u("transactionSubmitRequest", "Required parameter requestParameters.transactionSubmitRequest was null or undefined when calling submitTransaction.");
    const r = {}, i = {};
    i["Content-Type"] = "application/json";
    const o = await this.request({
      path: "/transaction/submit",
      method: "POST",
      headers: i,
      query: r,
      body: fn(e.transactionSubmitRequest)
    }, n);
    return new c(o, (a) => _n(a));
  }
  async submitTransaction(e, n) {
    return await (await this.submitTransactionRaw(e, n)).value();
  }
  async transactionDetailsRaw(e, n) {
    if (e.transactionDetailsRequest === null || e.transactionDetailsRequest === void 0)
      throw new u("transactionDetailsRequest", "Required parameter requestParameters.transactionDetailsRequest was null or undefined when calling transactionDetails.");
    const r = {}, i = {};
    i["Content-Type"] = "application/json";
    const o = await this.request({
      path: "/transaction/details",
      method: "POST",
      headers: i,
      query: r,
      body: He(e.transactionDetailsRequest)
    }, n);
    return new c(o, (a) => Qe(a));
  }
  async transactionDetails(e, n) {
    return await (await this.transactionDetailsRaw(e, n)).value();
  }
  async transactionStatusRaw(e, n) {
    if (e.transactionStatusRequest === null || e.transactionStatusRequest === void 0)
      throw new u("transactionStatusRequest", "Required parameter requestParameters.transactionStatusRequest was null or undefined when calling transactionStatus.");
    const r = {}, i = {};
    i["Content-Type"] = "application/json";
    const o = await this.request({
      path: "/transaction/status",
      method: "POST",
      headers: i,
      query: r,
      body: an(e.transactionStatusRequest)
    }, n);
    return new c(o, (a) => un(a));
  }
  async transactionStatus(e, n) {
    return await (await this.transactionStatusRaw(e, n)).value();
  }
}
export {
  gt as AccountIdentifierFromJSON,
  St as AccountIdentifierFromJSONTyped,
  Tt as AccountIdentifierToJSON,
  ft as BASE_PATH,
  h as BaseAPI,
  Tn as BlobApiResponse,
  On as COLLECTION_FORMATS,
  _t as Configuration,
  pt as DefaultConfig,
  Jn as EcdsaSecp256k1PublicKeyFromJSON,
  b as EcdsaSecp256k1PublicKeyFromJSONTyped,
  ht as EcdsaSecp256k1PublicKeyToJSON,
  Fn as EddsaEd25519PublicKeyFromJSON,
  I as EddsaEd25519PublicKeyFromJSONTyped,
  Nt as EddsaEd25519PublicKeyToJSON,
  co as EntityApi,
  In as EntityDetailsRequestFromJSON,
  Jt as EntityDetailsRequestFromJSONTyped,
  Et as EntityDetailsRequestToJSON,
  An as EntityDetailsResponseAccountComponentDetailsFromJSON,
  v as EntityDetailsResponseAccountComponentDetailsFromJSONTyped,
  Dt as EntityDetailsResponseAccountComponentDetailsToJSON,
  Hn as EntityDetailsResponseAllOfFromJSON,
  Kt as EntityDetailsResponseAllOfFromJSONTyped,
  Qn as EntityDetailsResponseAllOfToJSON,
  k as EntityDetailsResponseDetailsFromJSON,
  Pt as EntityDetailsResponseDetailsFromJSONTyped,
  C as EntityDetailsResponseDetailsToJSON,
  vn as EntityDetailsResponseDetailsType,
  N as EntityDetailsResponseDetailsTypeFromJSON,
  Ft as EntityDetailsResponseDetailsTypeFromJSONTyped,
  qn as EntityDetailsResponseDetailsTypeToJSON,
  Lt as EntityDetailsResponseFromJSON,
  Gt as EntityDetailsResponseFromJSONTyped,
  kn as EntityDetailsResponseFungibleResourceDetailsFromJSON,
  q as EntityDetailsResponseFungibleResourceDetailsFromJSONTyped,
  bt as EntityDetailsResponseFungibleResourceDetailsToJSON,
  Xn as EntityDetailsResponseMetadataAllOfFromJSON,
  Ut as EntityDetailsResponseMetadataAllOfFromJSONTyped,
  Yn as EntityDetailsResponseMetadataAllOfToJSON,
  M as EntityDetailsResponseMetadataFromJSON,
  Ct as EntityDetailsResponseMetadataFromJSONTyped,
  L as EntityDetailsResponseMetadataToJSON,
  Gn as EntityDetailsResponseNonFungibleResourceDetailsFromJSON,
  P as EntityDetailsResponseNonFungibleResourceDetailsFromJSONTyped,
  jn as EntityDetailsResponseNonFungibleResourceDetailsIdsAllOfFromJSON,
  Vt as EntityDetailsResponseNonFungibleResourceDetailsIdsAllOfFromJSONTyped,
  tr as EntityDetailsResponseNonFungibleResourceDetailsIdsAllOfToJSON,
  vt as EntityDetailsResponseNonFungibleResourceDetailsIdsFromJSON,
  qt as EntityDetailsResponseNonFungibleResourceDetailsIdsFromJSONTyped,
  x as EntityDetailsResponseNonFungibleResourceDetailsIdsItemFromJSON,
  It as EntityDetailsResponseNonFungibleResourceDetailsIdsItemFromJSONTyped,
  A as EntityDetailsResponseNonFungibleResourceDetailsIdsItemToJSON,
  xt as EntityDetailsResponseNonFungibleResourceDetailsIdsToJSON,
  At as EntityDetailsResponseNonFungibleResourceDetailsToJSON,
  zn as EntityDetailsResponseToJSON,
  w as EntityMetadataItemFromJSON,
  kt as EntityMetadataItemFromJSONTyped,
  J as EntityMetadataItemToJSON,
  nr as EntityOverviewRequestFromJSON,
  $t as EntityOverviewRequestFromJSONTyped,
  zt as EntityOverviewRequestToJSON,
  ur as EntityOverviewResponseAllOfFromJSON,
  Zt as EntityOverviewResponseAllOfFromJSONTyped,
  cr as EntityOverviewResponseAllOfToJSON,
  G as EntityOverviewResponseEntityItemFromJSON,
  Wt as EntityOverviewResponseEntityItemFromJSONTyped,
  Bt as EntityOverviewResponseEntityItemMetadataFromJSON,
  Ht as EntityOverviewResponseEntityItemMetadataFromJSONTyped,
  Qt as EntityOverviewResponseEntityItemMetadataToJSON,
  K as EntityOverviewResponseEntityItemToJSON,
  Xt as EntityOverviewResponseFromJSON,
  Yt as EntityOverviewResponseFromJSONTyped,
  sr as EntityOverviewResponseToJSON,
  lr as EntityResourcesRequestFromJSON,
  jt as EntityResourcesRequestFromJSONTyped,
  te as EntityResourcesRequestToJSON,
  gr as EntityResourcesResponseAllOfFromJSON,
  ae as EntityResourcesResponseAllOfFromJSONTyped,
  Sr as EntityResourcesResponseAllOfToJSON,
  oe as EntityResourcesResponseFromJSON,
  se as EntityResourcesResponseFromJSONTyped,
  hr as EntityResourcesResponseFungibleResourcesAllOfFromJSON,
  ue as EntityResourcesResponseFungibleResourcesAllOfFromJSONTyped,
  Nr as EntityResourcesResponseFungibleResourcesAllOfToJSON,
  $ as EntityResourcesResponseFungibleResourcesFromJSON,
  ne as EntityResourcesResponseFungibleResourcesFromJSONTyped,
  U as EntityResourcesResponseFungibleResourcesItemFromJSON,
  ee as EntityResourcesResponseFungibleResourcesItemFromJSONTyped,
  V as EntityResourcesResponseFungibleResourcesItemToJSON,
  z as EntityResourcesResponseFungibleResourcesToJSON,
  Jr as EntityResourcesResponseNonFungibleResourcesAllOfFromJSON,
  ce as EntityResourcesResponseNonFungibleResourcesAllOfFromJSONTyped,
  Er as EntityResourcesResponseNonFungibleResourcesAllOfToJSON,
  Q as EntityResourcesResponseNonFungibleResourcesFromJSON,
  ie as EntityResourcesResponseNonFungibleResourcesFromJSONTyped,
  B as EntityResourcesResponseNonFungibleResourcesItemFromJSON,
  re as EntityResourcesResponseNonFungibleResourcesItemFromJSONTyped,
  H as EntityResourcesResponseNonFungibleResourcesItemToJSON,
  W as EntityResourcesResponseNonFungibleResourcesToJSON,
  Or as EntityResourcesResponseToJSON,
  br as ErrorResponseFromJSON,
  le as ErrorResponseFromJSONTyped,
  Ir as ErrorResponseToJSON,
  Rt as FetchError,
  de as GatewayErrorFromJSON,
  y as GatewayErrorFromJSONTyped,
  O as GatewayErrorToJSON,
  kr as GatewayInfoResponseAllOfFromJSON,
  ye as GatewayInfoResponseAllOfFromJSONTyped,
  Cr as GatewayInfoResponseAllOfToJSON,
  pe as GatewayInfoResponseFromJSON,
  me as GatewayInfoResponseFromJSONTyped,
  X as GatewayInfoResponseGatewayApiVersionsFromJSON,
  fe as GatewayInfoResponseGatewayApiVersionsFromJSONTyped,
  Y as GatewayInfoResponseGatewayApiVersionsToJSON,
  Z as GatewayInfoResponseTargetLedgerStateFromJSON,
  _e as GatewayInfoResponseTargetLedgerStateFromJSONTyped,
  j as GatewayInfoResponseTargetLedgerStateToJSON,
  Ar as GatewayInfoResponseToJSON,
  Ur as InternalServerErrorAllOfFromJSON,
  Oe as InternalServerErrorAllOfFromJSONTyped,
  Vr as InternalServerErrorAllOfToJSON,
  Lr as InternalServerErrorFromJSON,
  tt as InternalServerErrorFromJSONTyped,
  Gr as InternalServerErrorToJSON,
  Wr as InvalidRequestErrorAllOfFromJSON,
  ge as InvalidRequestErrorAllOfFromJSONTyped,
  Xr as InvalidRequestErrorAllOfToJSON,
  Br as InvalidRequestErrorFromJSON,
  rt as InvalidRequestErrorFromJSONTyped,
  Hr as InvalidRequestErrorToJSON,
  Zr as InvalidTransactionErrorFromJSON,
  it as InvalidTransactionErrorFromJSONTyped,
  jr as InvalidTransactionErrorToJSON,
  c as JSONApiResponse,
  d as LedgerStateFromJSON,
  Mt as LedgerStateFromJSONTyped,
  ei as LedgerStateMixinFromJSON,
  Se as LedgerStateMixinFromJSONTyped,
  ni as LedgerStateMixinToJSON,
  l as LedgerStateToJSON,
  ai as NotSyncedUpErrorAllOfFromJSON,
  Te as NotSyncedUpErrorAllOfFromJSONTyped,
  ui as NotSyncedUpErrorAllOfToJSON,
  ii as NotSyncedUpErrorFromJSON,
  ot as NotSyncedUpErrorFromJSONTyped,
  oi as NotSyncedUpErrorToJSON,
  f as PartialLedgerStateIdentifierFromJSON,
  wt as PartialLedgerStateIdentifierFromJSONTyped,
  _ as PartialLedgerStateIdentifierToJSON,
  he as PublicKeyFromJSON,
  Ne as PublicKeyFromJSONTyped,
  we as PublicKeyToJSON,
  ci as PublicKeyType,
  di as PublicKeyTypeFromJSON,
  Je as PublicKeyTypeFromJSONTyped,
  li as PublicKeyTypeToJSON,
  _i as RecentTransactionsRequestFromJSON,
  Ee as RecentTransactionsRequestFromJSONTyped,
  Fe as RecentTransactionsRequestToJSON,
  Me as RecentTransactionsResponseFromJSON,
  Le as RecentTransactionsResponseFromJSONTyped,
  Si as RecentTransactionsResponseToJSON,
  u as RequiredError,
  Ot as ResponseError,
  hi as ResultSetCursorMixinFromJSON,
  Ge as ResultSetCursorMixinFromJSONTyped,
  Ni as ResultSetCursorMixinToJSON,
  lo as StatusApi,
  hn as TextApiResponse,
  ve as TokenAmountFromJSON,
  qe as TokenAmountFromJSONTyped,
  xe as TokenAmountToJSON,
  De as TokenIdentifierFromJSON,
  be as TokenIdentifierFromJSONTyped,
  Ie as TokenIdentifierToJSON,
  Ji as TokenPropertiesFromJSON,
  Ke as TokenPropertiesFromJSONTyped,
  Ei as TokenPropertiesToJSON,
  fo as TransactionApi,
  st as TransactionDetailsFromJSON,
  Ue as TransactionDetailsFromJSONTyped,
  qi as TransactionDetailsRequestFromJSON,
  Be as TransactionDetailsRequestFromJSONTyped,
  He as TransactionDetailsRequestToJSON,
  ki as TransactionDetailsResponseAllOfFromJSON,
  Xe as TransactionDetailsResponseAllOfFromJSONTyped,
  Ci as TransactionDetailsResponseAllOfToJSON,
  Qe as TransactionDetailsResponseFromJSON,
  We as TransactionDetailsResponseFromJSONTyped,
  Ai as TransactionDetailsResponseToJSON,
  at as TransactionDetailsToJSON,
  R as TransactionInfoFromJSON,
  Ce as TransactionInfoFromJSONTyped,
  g as TransactionInfoToJSON,
  S as TransactionLookupIdentifierFromJSON,
  ze as TransactionLookupIdentifierFromJSONTyped,
  T as TransactionLookupIdentifierToJSON,
  Di as TransactionLookupOrigin,
  Ve as TransactionLookupOriginFromJSON,
  $e as TransactionLookupOriginFromJSONTyped,
  bi as TransactionLookupOriginToJSON,
  Ui as TransactionNotFoundErrorAllOfFromJSON,
  Ye as TransactionNotFoundErrorAllOfFromJSONTyped,
  Vi as TransactionNotFoundErrorAllOfToJSON,
  Li as TransactionNotFoundErrorFromJSON,
  ut as TransactionNotFoundErrorFromJSONTyped,
  Gi as TransactionNotFoundErrorToJSON,
  Ze as TransactionPreviewRequestFlagsFromJSON,
  je as TransactionPreviewRequestFlagsFromJSONTyped,
  tn as TransactionPreviewRequestFlagsToJSON,
  Bi as TransactionPreviewRequestFromJSON,
  en as TransactionPreviewRequestFromJSONTyped,
  nn as TransactionPreviewRequestToJSON,
  rn as TransactionPreviewResponseFromJSON,
  on as TransactionPreviewResponseFromJSONTyped,
  Qi as TransactionPreviewResponseToJSON,
  Ae as TransactionStatusFromJSON,
  Pe as TransactionStatusFromJSONTyped,
  Xi as TransactionStatusRequestFromJSON,
  sn as TransactionStatusRequestFromJSONTyped,
  an as TransactionStatusRequestToJSON,
  to as TransactionStatusResponseAllOfFromJSON,
  dn as TransactionStatusResponseAllOfFromJSONTyped,
  eo as TransactionStatusResponseAllOfToJSON,
  un as TransactionStatusResponseFromJSON,
  cn as TransactionStatusResponseFromJSONTyped,
  Zi as TransactionStatusResponseToJSON,
  yi as TransactionStatusStatusEnum,
  ke as TransactionStatusToJSON,
  ro as TransactionSubmitRequestFromJSON,
  ln as TransactionSubmitRequestFromJSONTyped,
  fn as TransactionSubmitRequestToJSON,
  _n as TransactionSubmitResponseFromJSON,
  pn as TransactionSubmitResponseFromJSONTyped,
  oo as TransactionSubmitResponseToJSON,
  et as ValidationErrorsAtPathFromJSON,
  Re as ValidationErrorsAtPathFromJSONTyped,
  nt as ValidationErrorsAtPathToJSON,
  ao as ValidatorIdentifierFromJSON,
  mn as ValidatorIdentifierFromJSONTyped,
  uo as ValidatorIdentifierToJSON,
  Sn as VoidApiResponse,
  gn as canConsumeForm,
  s as exists,
  Nn as instanceOfAccountIdentifier,
  wn as instanceOfEcdsaSecp256k1PublicKey,
  En as instanceOfEddsaEd25519PublicKey,
  bn as instanceOfEntityDetailsRequest,
  $n as instanceOfEntityDetailsResponse,
  xn as instanceOfEntityDetailsResponseAccountComponentDetails,
  Bn as instanceOfEntityDetailsResponseAllOf,
  Pn as instanceOfEntityDetailsResponseFungibleResourceDetails,
  Un as instanceOfEntityDetailsResponseMetadata,
  Wn as instanceOfEntityDetailsResponseMetadataAllOf,
  Ln as instanceOfEntityDetailsResponseNonFungibleResourceDetails,
  Mn as instanceOfEntityDetailsResponseNonFungibleResourceDetailsIds,
  Zn as instanceOfEntityDetailsResponseNonFungibleResourceDetailsIdsAllOf,
  Cn as instanceOfEntityDetailsResponseNonFungibleResourceDetailsIdsItem,
  Kn as instanceOfEntityMetadataItem,
  er as instanceOfEntityOverviewRequest,
  or as instanceOfEntityOverviewResponse,
  ar as instanceOfEntityOverviewResponseAllOf,
  ir as instanceOfEntityOverviewResponseEntityItem,
  rr as instanceOfEntityOverviewResponseEntityItemMetadata,
  dr as instanceOfEntityResourcesRequest,
  yr as instanceOfEntityResourcesResponse,
  Rr as instanceOfEntityResourcesResponseAllOf,
  _r as instanceOfEntityResourcesResponseFungibleResources,
  Tr as instanceOfEntityResourcesResponseFungibleResourcesAllOf,
  fr as instanceOfEntityResourcesResponseFungibleResourcesItem,
  mr as instanceOfEntityResourcesResponseNonFungibleResources,
  wr as instanceOfEntityResourcesResponseNonFungibleResourcesAllOf,
  pr as instanceOfEntityResourcesResponseNonFungibleResourcesItem,
  Dr as instanceOfErrorResponse,
  Fr as instanceOfGatewayError,
  xr as instanceOfGatewayInfoResponse,
  Pr as instanceOfGatewayInfoResponseAllOf,
  vr as instanceOfGatewayInfoResponseGatewayApiVersions,
  qr as instanceOfGatewayInfoResponseTargetLedgerState,
  Mr as instanceOfInternalServerError,
  Kr as instanceOfInternalServerErrorAllOf,
  zr as instanceOfInvalidRequestError,
  Qr as instanceOfInvalidRequestErrorAllOf,
  Yr as instanceOfInvalidTransactionError,
  Vn as instanceOfLedgerState,
  ti as instanceOfLedgerStateMixin,
  ri as instanceOfNotSyncedUpError,
  si as instanceOfNotSyncedUpErrorAllOf,
  Dn as instanceOfPartialLedgerStateIdentifier,
  fi as instanceOfRecentTransactionsRequest,
  gi as instanceOfRecentTransactionsResponse,
  Ti as instanceOfResultSetCursorMixin,
  mi as instanceOfTokenAmount,
  pi as instanceOfTokenIdentifier,
  wi as instanceOfTokenProperties,
  Fi as instanceOfTransactionDetails,
  vi as instanceOfTransactionDetailsRequest,
  xi as instanceOfTransactionDetailsResponse,
  Pi as instanceOfTransactionDetailsResponseAllOf,
  Ri as instanceOfTransactionInfo,
  Ii as instanceOfTransactionLookupIdentifier,
  Mi as instanceOfTransactionNotFoundError,
  Ki as instanceOfTransactionNotFoundErrorAllOf,
  zi as instanceOfTransactionPreviewRequest,
  $i as instanceOfTransactionPreviewRequestFlags,
  Hi as instanceOfTransactionPreviewResponse,
  Oi as instanceOfTransactionStatus,
  Wi as instanceOfTransactionStatusRequest,
  Yi as instanceOfTransactionStatusResponse,
  ji as instanceOfTransactionStatusResponseAllOf,
  no as instanceOfTransactionSubmitRequest,
  io as instanceOfTransactionSubmitResponse,
  $r as instanceOfValidationErrorsAtPath,
  so as instanceOfValidatorIdentifier,
  Rn as mapValues,
  F as querystring
};
