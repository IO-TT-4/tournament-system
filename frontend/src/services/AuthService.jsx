import { api } from '../api/api';
import { handleError } from '../helpers/ErrorHandler';

export async function loginAPI(username, password) {
  try {
    const data = api.post('/login', {
      username: username,
      password: password,
    });
    return data;
  } catch (error) {
    handleError(error);
  }
}

export async function registerApi(email, username, password) {
  try {
    const data = api.post('/register', {
      username: username,
      password: password,
      email: email,
    });
    return data;
  } catch (error) {
    handleError(error);
  }
}
