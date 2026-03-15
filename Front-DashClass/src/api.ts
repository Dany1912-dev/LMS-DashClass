const BASE_URL = import.meta.env.VITE_API_URL;

export const API = {
  register: `${BASE_URL}/api/Auth/register`,
  login: `${BASE_URL}/api/Auth/login`,
};
