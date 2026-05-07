/* Api Management */
class Api
{
    constructor(args) {
        args = args || {};
        this.apiRoot = args.apiRoot || session.apiRoot;

        this.cache = {
            meta: [],
            resources: [],
            resourceLoads: []
        };
        this.app = args.app || session.app;
        this.file = {
            upload: async (to, file, callback) => {
                return new Promise((resolve, reject) => {
                    try {
                        let reader = new FileReader();
                        reader.onload = () => this.onFileReaderLoad(to, file, reader, resolve, callback);
                        reader.readAsArrayBuffer(file);
                    } catch(err) {
                        reject(err);
                    }
                });
            },

            destroy: async (path) => {
                await this.destroy("DMS/" + path)
                    .then(() => notification.success("File deleted."));
            }
        };
    }

    onFileReaderLoad(to, file, reader, resolve, callback) {
        let xhr = new XMLHttpRequest();
        xhr.open('POST', this.apiRoot + "DMS/" + to, true);
        xhr.setRequestHeader("Content-Type", file.type);

        xhr.onreadystatechange = () =>
            this.onReadyStateChange(xhr, file, resolve, callback);

        xhr.send(new Uint8Array(reader.result));
    }

    onReadyStateChange(xhr, file, resolve, callback) {
        switch (xhr.readyState) {
            case 2: //HEADERS_RECEIVED
                if (notification)
                    notification.info("Uploading file " + file.name + " ...");
                break;
            case 4: //DONE
                if (notification) {
                    notification.success(file.name + " has been uploaded.");
                } else {
                    notification.fail(file.name + " failed to upload.");
                    log(file.name + " upload fail.", "success");
                }
                resolve(file);
                if (callback)
                    callback(file);
                break;
            default:
                break;
        }
    }

    async login(user, pass, keepToken) {
        return this.send("POST", "Account/Login", {
            User: user,
            Pass: pass
        }).then((token) => {
            if (keepToken)
                this.token = token.id;
            return token;
        }).catch((e) => {
            error(e);
            return false;
        });
    }

    async logout() {
        if (this.token)
            delete this.token;

        return this.send("POST", "Account/Logout", '').catch((e) => error(e));
    }

    async register(details) {
        return api.post("Account/Register", details);
    }

    addToMetaCache(metaSet) {
        for (let i in metaSet) {
            let ctx = metaSet[i];
            let existing = this.cache.meta.filter(x => x.Name == ctx.Name);

            if (existing.length > 0) {
                for (let t in ctx.Types) {
                    existing[0].Types.push(ctx.Types[t]);
                }
            }
            else
                this.cache.meta.push(ctx);
        }
    }

    addToResourceCache(resourceSet) {
        for (let r in resourceSet)
            this.cache.resources.push(resourceSet[r]);
    }

    getType(endpointRef) {
        let context = endpointRef.split('/')[0];
        let typeName = endpointRef.split('/')[1];

        if (this.cache.meta.filter(x => x.Name === context).length === 0)
            throw new Error("Missing " + context + " type information in meta cache");

        let typeGroup = this.cache.meta.filter(x => x.Name === context)[0];

        if (typeGroup.Types.filter(x => x.ServerTypeName === typeName).length === 0)
            throw new Error("Missing " + endpointRef + " type information in meta cache");

        return typeGroup.Types.filter(x => x.ServerTypeName === typeName)[0];
    }

    getResource(key, name, culture) {
        culture = culture || "";
        if (!key || !name) {
            return {
                Key: key,
                Name: name,
                DisplayName: name,
                ShortDisplayName: name,
                Description: name
            };
        }

        const subSet = this.cache.resources.filter(r => r.Key.toLowerCase() === key.toLowerCase() && r.Name.toLowerCase() === name.toLowerCase());
        const resultSet = subSet
            .filter(r => culture === r.Culture || (culture.indexOf(r.Culture) > -1) || r.Culture === '')
            .sort((a, b) => b.Culture.length - a.Culture.length);

        return (resultSet.length > 0)
            ? resultSet[0]
            : {
                Key: key,
                Name: name,
                DisplayName: name,
                ShortDisplayName: name,
                Description: name
            };
    }

    async send(type, query, data, contentType) {
        // if no query provided set to blank //
        query = query || '';
        // construct the promise //
        // if not return promise for client to handle //
        return new Promise((resolve, reject) => {
            // create the config that will be used by the ajax call //
            const ajaxConfig = {
                type: type,
                contentType: contentType || 'application/json',
                crossDomain: window.location.href.indexOf(this.apiRoot) > -1,
                url: this.apiRoot + query,
                success: resolve,
                error: reject,
                beforeSend: (xhr) => this.beforeSend(xhr, contentType),
                //timeout: this.timeout
            };

            // if we have been provided data, then add it to the call //
            if (data) {
                ajaxConfig.data = JSON.stringify(data);
            }

            $.ajax(ajaxConfig);
        });
    }

    async sendRaw(type, query, data) {
        // if no query provided set to blank //
        query = query || '';
        // construct the promise //
        // if not return promise for client to handle //
        return new Promise((resolve, reject) => {
            // create the config that will be used by the ajax call //
            const ajaxConfig = {
                type: type,
                contentType: "text/plain",
                crossDomain: window.location.href.indexOf(this.apiRoot) > -1,
                url: this.apiRoot + query,
                success: resolve,
                error: reject,
                beforeSend: (xhr) => this.beforeSend(xhr, "text/plain"),
                data: data
            };

            $.ajax(ajaxConfig);
        });
    }

    beforeSend(xhr, contentType) {
        xhr.setRequestHeader("Content-Type", contentType || "application/json;odata=minimalmetadata");
        xhr.setRequestHeader("Accept", contentType || "application/json;odata=minimalmetadata");

        if (this.token) {
            xhr.setRequestHeader("Authorization", "bearer " + this.token);
        }
    }

    async get(query) {
        return this.send("GET", query, null);
    }

    async add(query, model) {
        return this.send("POST", query, model);
    }

    async update(query, model) {
        return this.send("PUT", query, model);
    }

    async post(query, model) {
        return this.send("POST", query, model);
    }

    async put(query, model) {
        return this.send("PUT", query, model);
    }

    async destroy(query) {
        return this.send("DELETE", query, null);
    }

    success() {
        notification.success("Request Complete!");
    }
}

window.api = new Api({
    baseUrl: session.apiRoot,
    token: session.token
});