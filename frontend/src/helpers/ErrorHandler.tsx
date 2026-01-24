import axios from 'axios';
import i18next from 'i18next';
import { toast } from 'react-toastify';

export function handleError(error: any) {
  if (axios.isAxiosError(error)) {
    console.log(error?.response?.data?.code);

    if (error?.response?.data?.code == 'INVALID_CREDENTIALS') {
      toast.error(i18next.t('InvalidCredentials', { ns: 'toast' }));
    } else {
      toast.error(i18next.t('serverError', { ns: 'toast' }));
    }
  }
}
