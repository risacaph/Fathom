import {getBaseUrl} from "src/app/_providers/base-url.provider";

const BASE_URL = getBaseUrl();

export const environment = {
  production: true,
  apiUrl: `${BASE_URL}api/`,
  hubUrl:`${BASE_URL}hubs/`,
  buyLink: '',
  manageLink: ''
};
