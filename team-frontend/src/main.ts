import Vue from 'vue';
import './plugins/quizr-helpers';
import App from './App.vue';
import router from './router/index';
import store from './store/index';
import BootstrapVue from 'bootstrap-vue';
import { library } from '@fortawesome/fontawesome-svg-core'
import { faSignOutAlt, faTrashAlt, faPlay, faPause, faPauseCircle, faStop, faPowerOff, faUser, faArrowLeft, faArrowRight } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import axios from 'axios';
import i18n from './plugins/i18n'

library.add(faSignOutAlt, faTrashAlt, faPlay, faPause, faStop, faPowerOff, faUser, faArrowLeft, faArrowRight);
Vue.component('font-awesome-icon', FontAwesomeIcon);

Vue.config.productionTip = false;

Vue.use(BootstrapVue, {
  BToast: {
    toaster: 'b-toaster-bottom-right'
  }
});

const instanceUserApi = axios.create({
  baseURL: process.env.VUE_APP_BACKEND_URI
});
instanceUserApi.interceptors.request.use(
  cfg => {
    if (localStorage.getItem('token')) {
      cfg.headers.common.Authorization = 'Bearer ' + localStorage.getItem('token');
    }
    return cfg;
  },
  err => {
    // Do something with request error
    return Promise.reject(err);
  }
);


Vue.prototype.$axios = instanceUserApi;

new Vue({
  router,
  store,
  i18n,
  render: h => h(App)
}).$mount('#app');
